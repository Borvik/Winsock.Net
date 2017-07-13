using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Treorisoft.Net.Utilities;

namespace Treorisoft.Net
{
    internal class WinSocket
    {
        const int READ_WRITE_TIMEOUT = 500;

        public WinSocket(Winsock parent)
        {
            Parent = parent;
        }

        #region Fields

        // Stores bytes between stream receiving and being processed
        //private ReceiveBuffer Buffer = new ReceiveBuffer();
        private bool closing = false;
        private PacketHeader Header = new PacketHeader();
        // Stores objects received that can be requested via winsock Get/Peek methods
        private Queue<object> ReceivedBuffer = new Queue<object>();
        // Temporarily stores data on incoming files
        private Dictionary<Guid, FileData> IncomingFiles = new Dictionary<Guid, FileData>();
        private bool IsProcessingIncomingData = false;
        private bool IsSending = false;
        private Winsock Parent;
        // Used to store bytes during processing so processing can work on more than one packet
        private ByteBufferCollection ProcessingByteBuffer = new ByteBufferCollection();
        // Stores received packets just before processing
        private Deque<ReceivedPacket> ReceivedPackets = new Deque<ReceivedPacket>();
        // Stores packets going out
        private Deque<SendPacket> SendBuffer = new Deque<SendPacket>();
        private AsyncSocket socket;
        private Stream socketStream;

        #endregion

        #region Properties

        internal IPEndPoint LocalEndPoint { get { return socket?.LocalEndPoint as IPEndPoint; } }
        internal IPEndPoint RemoteEndPoint { get { return socket?.RemoteEndPoint as IPEndPoint; } }

        #endregion

        #region Public Methods

        #region Client/Server Methods

        /// <summary>
        /// Closes the socket if it is open or listening.
        /// </summary>
        public void Close()
        {
            if (closing || Parent.State == State.Closed) return;
            closing = true;

            try
            {
                var oldState = Parent.State;
                if (oldState == State.Connected || oldState == State.Listening)
                {
                    Parent.ChangeState(State.Closing);
                    if (socketStream != null)
                        socketStream.Dispose(); // Should take care of MOST instances, this Dispose also closes the socket.
                    else if (socket != null && oldState == State.Listening)
                        socket.Close(); // Listeners don't create a stream, so this handles those
                    else if (socket != null && socket.IsConnected)
                    {
                        // Edge cases and errors could potentially leave us with a connected socket but without a stream.
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    socketStream = null;
                    socket = null;
                }
                Parent.ChangeState(State.Closed);
                if (oldState == State.Connected)
                    Parent.OnDisconnected();
            }
            catch (Exception ex)
            {
                Parent.OnErrorReceived(Parent, ex.AsEventArgs());
            }
            finally
            {
                closing = false;
            }
        }

        /// <summary>
        /// Removes and returns the next <see cref="object"/> in the receive buffer.
        /// </summary>
        /// <returns>The received <see cref="object"/>.</returns>
        public object Get()
        {
            lock (((ICollection)ReceivedBuffer).SyncRoot)
            {
                if (ReceivedBuffer.Count == 0) return null;
                return ReceivedBuffer.Dequeue();
            }
        }

        /// <summary>
        /// Returns the next <see cref="object"/> in the receive buffer, without removing it.
        /// </summary>
        /// <returns>The received <see cref="object"/>.</returns>
        public object Peek()
        {
            lock (((ICollection)ReceivedBuffer).SyncRoot)
            {
                if (ReceivedBuffer.Count == 0) return null;
                return ReceivedBuffer.Peek();
            }
        }

        /// <summary>
        /// Sends a sequence of bytes to the connected socket.
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array of data to send.</param>
        public async void Send(byte[] data)
        {
            IPEndPoint destination = await GetDestination();

            if (!Parent.LegacySupport)
                PacketHeader.AddHeader(ref data);

            var dataPacket = new SendPacket(destination, data);
            lock (SendBuffer.SyncRoot)
                SendBuffer.PushBack(dataPacket);

            StartSendLoop();
        }
        /// <summary>
        /// Sends a file to the connected socket.
        /// </summary>
        /// <param name="data">The <see cref="FileData"/> with the information regarding the file to send.</param>
        public async void Send(FileData data)
        {
            IPEndPoint destination = await GetDestination();

            var dataPacket = new SendPacket(destination, null, data, Parent.BufferSize, Parent.LegacySupport);
            lock (SendBuffer.SyncRoot)
                SendBuffer.PushBack(dataPacket);

            StartSendLoop();
        }

        #endregion

        #region Client Methods

        /// <summary>
        /// Establishes a connection to a remote host.
        /// </summary>
        /// <param name="remoteHostOrIp">A value containing the Hostname or IP address of the remote host.</param>
        /// <param name="remotePort">A value indicating the port on the remote host to connect to.</param>
        public void Connect(string remoteHostOrIp, int remotePort)
        {
            Connect(remoteHostOrIp, remotePort, null);
        }
        /// <summary>
        /// Establishes a connection to a remote host.
        /// </summary>
        /// <param name="remoteHostOrIp">A value containing the Hostname or IP address of the remote host.</param>
        /// <param name="remotePort">A value indicating the port on the remote host to connect to.</param>
        /// <param name="sslHost">The name of the host to validate the certificate for.</param>
        public async void Connect(string remoteHostOrIp, int remotePort, string sslHost)
        {
            if (Parent.State != State.Closed)
                throw new WinsockException("Cannot connect to a remote host when not Closed.");

            /**
             * First we need to make sure we have an IP address.
             * If not - we need to try and resolve the fully qualified domain.
             */
            Parent.ChangeState(State.ResolvingHost);
            IPAddress resolvedIP = null;
            if (!IPAddress.TryParse(remoteHostOrIp, out resolvedIP))
            {
                IPHostEntry entry = await Dns.GetHostEntryAsync(remoteHostOrIp);
                if (entry == null || entry.AddressList.Length == 0)
                {
                    string name = (entry != null) ? entry.HostName : remoteHostOrIp;
                    throw new WinsockException(string.Format("Hostname \"{0}\" could not be resolved.", name));
                }
                resolvedIP = entry.AddressList[0];
            }
            Parent.ChangeState(State.HostResolved);

            /**
             * Take our IP address and attempt to create the connection.
             * Upon successfull connections - different BeginReceives could be called
             * depending on if this was an attempt at a SECURE connection.
             */
            IPEndPoint endPoint = new IPEndPoint(resolvedIP, remotePort);
            socket = new AsyncSocket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Parent.ChangeState(State.Connecting);

            await socket.ConnectAsync(endPoint);
            Parent.ChangeState(State.Connected);
            Parent.OnConnected(endPoint);
            if (sslHost != null) BeginReceive(false, sslHost);
            else BeginReceive(false);
        }

        #endregion

        #region Server Methods

        /// <summary>
        /// Accepts an incoming connection and starts the data listener.
        /// </summary>
        /// <param name="client">The client to accept.</param>
        /// <returns>true on success; otherwise false.</returns>
        public bool Accept(Socket client)
        {
            if (Parent.State != State.Closed)
                throw new WinsockException("Cannot accept a connection while the State is not closed.");

            try
            {
                socket = new AsyncSocket(client);
                
                Parent.ChangeLocalPort(LocalEndPoint.Port);
                Parent.ChangeRemoteHost(RemoteEndPoint.Address.ToString());
                Parent.ChangeRemotePort(RemoteEndPoint.Port);

                Parent.ChangeState(State.Connected);
                Parent.OnConnected(RemoteEndPoint);
                BeginReceive(true);
                return true;
            }
            catch (Exception ex)
            {
                Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                return false;
            }
        }

        /// <summary>
        /// Places the <see cref="WinSocket"/> into a listening state.
        /// </summary>
        /// <param name="ipAddress">An <see cref="IPAddress"/> containing the IP address to start listening on.  Null to listen on all available interfaces.</param>
        /// <param name="port">The port to start listening on.</param>
        /// <remarks>Threading hasn't really started yet, so allow exceptions to bubble-up.</remarks>
        public void Listen(IPAddress ipAddress, int port)
        {
            bool ipWasNull = (ipAddress == null);
            if (ipWasNull)
                ipAddress = Socket.OSSupportsIPv6 ? IPAddress.IPv6Any : IPAddress.Any;

            AddressFamily family = ipAddress.AddressFamily;
            SocketType stype = (Parent.Protocol == Protocol.Tcp) ? SocketType.Stream : SocketType.Dgram;
            ProtocolType ptype = (Parent.Protocol == Protocol.Tcp) ? ProtocolType.Tcp : ProtocolType.Udp;

            int portUsed = port;
            socket = new AsyncSocket(family, stype, ptype);
            if (ipWasNull && ipAddress == IPAddress.IPv6Any)
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
            if (ptype == ProtocolType.Udp)
                socket.SetSocketOption((family == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP), SocketOptionName.PacketInformation, true);

            var localEndpoint = new IPEndPoint(ipAddress, port);
            socket.Bind(localEndpoint);
            if (port == 0)
            {
                portUsed = ((IPEndPoint)socket.LocalEndPoint).Port;
                Parent.ChangeLocalPort(portUsed);
            }

            if (Parent.Protocol == Protocol.Tcp)
                (new Thread(ListenTCP)).Start();
            else // UPD
                (new Thread(ListenUDP)).Start();
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the receiving stream, for server or client with SSL or not.
        /// </summary>
        /// <param name="asServer">Whether we are receiving as a server or not.</param>
        /// <param name="serverName">If receiving as a client, the server name the certifcate should match.</param>
        private async void BeginReceive(bool asServer, string serverName = "")
        {
            try
            {
                if (asServer)
                {
                    /**
                     * Server receiving gets called from Accept()
                     * If SSL was configured - authenticate (setup) the server.
                     */
                    socketStream = new NetworkStream(socket.internalSocket, true);
                    if (Parent.SslProtocol != SslProtocols.None && Parent.Certificate != null)
                    {
                        socketStream = new SslStream(socketStream, false, new RemoteCertificateValidationCallback(ValidateCertificate));
                        await ((SslStream)socketStream).AuthenticateAsServerAsync(Parent.Certificate, false, Parent.SslProtocol, true);
                    }
                }
                else
                {
                    /**
                     * Client receiving gets called from Connect()
                     * If SSL a was configured - authenticate as a client.
                     */
                    socketStream = new NetworkStream(socket.internalSocket, true);
                    if (Parent.SslProtocol != SslProtocols.None)
                    {
                        socketStream = new SslStream(socketStream, false, new RemoteCertificateValidationCallback(ValidateCertificate), new LocalCertificateSelectionCallback(SelectUserCertificate));
                        await ((SslStream)socketStream).AuthenticateAsClientAsync(serverName, new X509CertificateCollection(), Parent.SslProtocol, true);
                    }
                }
                
                socketStream.ReadTimeout = READ_WRITE_TIMEOUT;
                socketStream.WriteTimeout = READ_WRITE_TIMEOUT;

                // Start a thread for buffer checks
                //(new Thread(DoBufferCheck)).Start();

                // Start a thread to check for weird disconnects
                (new Thread(DoSocketPoll)).Start();

                // Start a thread to receive data on
                (new Thread(DoStreamReceive)).Start();
            }
            catch (Exception ex)
            {
                Parent.OnErrorReceived(Parent, ex.AsEventArgs());
            }
        }

        ///// <summary>
        ///// Checks if there is data in the buffer.
        ///// If the buffer is full, or a timeout has elapsed then the
        ///// data is pushed off to be processed.
        ///// </summary>
        //private void DoBufferCheck()
        //{
        //    byte[] buffer = null;
        //    IPEndPoint endPoint = null;
        //    Stopwatch stopwatch = Stopwatch.StartNew();
        //    int sleepInterval = 0;
        //    bool processBuffer = false;
        //    while (Parent.State == State.Connected)
        //    {
        //        lock (Buffer.SyncRoot)
        //        {
        //            processBuffer = Buffer.GetBuffer(Parent.BufferSize, READ_WRITE_TIMEOUT, out endPoint, out buffer);
        //            sleepInterval = processBuffer ? 0 : 500;
        //        }
        //        if (processBuffer)
        //            StartProcessIncoming(buffer, buffer.Length, endPoint);
        //        if (sleepInterval > 0)
        //            Thread.Sleep(sleepInterval);
        //    }
        //    lock (Buffer.SyncRoot)
        //    {
        //        while (Buffer.GetBuffer(Parent.BufferSize, READ_WRITE_TIMEOUT, out endPoint, out buffer))
        //            StartProcessIncoming(buffer, buffer.Length, endPoint);
        //    }
        //}

        //internal class BufferInfo
        //{
        //    public BufferInfo(byte[] buffer)
        //    {
        //        Stopwatch = Stopwatch.StartNew();
        //        Buffer = buffer;
        //    }
        //    public Stopwatch Stopwatch { get; set; }
        //    public byte[] Buffer { get; set; }
        //}
        //private class ReceiveBuffer : Dictionary<IPEndPoint, BufferInfo>
        //{
        //    public object SyncRoot { get { return ((ICollection)this).SyncRoot; } }
        //    public void AddTo(IPEndPoint endpoint, byte[] data)
        //    {
        //        if (Keys.Contains(endpoint))
        //            this[endpoint].Buffer = this[endpoint].Buffer.Concat(data).ToArray();
        //        else
        //            Add(endpoint, new BufferInfo(data));
        //    }

        //    public bool GetBuffer(int count, int timeout, out IPEndPoint endpoint, out byte[] buffer)
        //    {
        //        endpoint = null;
        //        buffer = null;

        //        var item = this.FirstOrDefault(b => b.Value.Stopwatch.ElapsedMilliseconds >= timeout || b.Value.Buffer.Length >= count);
        //        if (item.Equals(default(KeyValuePair<IPEndPoint, BufferInfo>)))
        //            return false;

        //        endpoint = item.Key;
        //        if(count == 0)
        //        {
        //            Remove(endpoint);
        //            buffer = item.Value.Buffer;
        //            return true;
        //        }

        //        var bytes = item.Value.Buffer;
        //        buffer = ArrayMethods.Shrink(ref bytes, count);
        //        if (bytes == null || bytes.Length == 0)
        //            Remove(endpoint);
        //        else
        //        {
        //            this[endpoint].Buffer = bytes;
        //            this[endpoint].Stopwatch.Restart();
        //        }
        //        return true;
        //    }
        //}

        /// <summary>
        /// Polls the connection socket to see if the connection is still open.
        /// If not - we close it on our end.
        /// Runs continously in its own thread until it is not longer connected.
        /// </summary>
        private void DoSocketPoll()
        {
            while (Parent.State == State.Connected)
            {
                bool connected = socket.IsConnected;
                if (!connected && Parent.State == State.Connected)
                {
                    Close();
                    break;
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Continously attempts to read data from the network stream, in it's own thread.
        /// At least until it is not connected anymore.
        /// </summary>
        private async void DoStreamReceive()
        {
            while (Parent.State == State.Connected)
            {
                byte[] buffer = new byte[Parent.BufferSize];
                int receivedSize = 0;
                try
                {
                    receivedSize = await socketStream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch (ObjectDisposedException ex)
                {
                    if (Parent.State == State.Connected)
                    {
                        Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                        Close();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                }

                if (receivedSize > 0)
                {
                    IPEndPoint endPoint = socket?.RemoteEndPoint as IPEndPoint;
                    Array.Resize(ref buffer, receivedSize);
                    // If enabling the receive buffer, StartProcessing must be commented out, and the lock and buffer uncommented
                    StartProcessIncoming(buffer, buffer.Length, endPoint);
                    //lock (((ICollection)Buffer).SyncRoot)
                    //Buffer.AddTo(endPoint, buffer);
                }
            }
        }

        /// <summary>
        /// Gets the destination address for UDP packets from the RemoteHost.
        /// </summary>
        /// <returns>An <see cref="IPEndPoint"/> containing the destination host and port.</returns>
        private async Task<IPEndPoint> GetDestination()
        {
            IPEndPoint destination = null;
            if (Parent.Protocol == Protocol.Udp)
            {
                IPAddress ip = null;
                if (IPAddress.TryParse(Parent.RemoteHost, out ip))
                    return new IPEndPoint(ip, Parent.RemotePort);

                try
                {
                    var host = await Dns.GetHostEntryAsync(Parent.RemoteHost);
                    ip = host.AddressList[0];
                    destination = new IPEndPoint(ip, Parent.RemotePort);
                }
                catch (Exception ex)
                {
                    Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                    return null;
                }
            }
            return destination;
        }

        /// <summary>
        /// Handles the header of an incoming file.
        /// Incoming files are stored in a temporary file until the DataArrival event fires and an action is decided upon.
        /// </summary>
        /// <param name="data">The <see cref="FileData"/> header of the incoming file.</param>
        /// <returns>Returns modified (with temp file information) <see cref="FileData"/> header to be used in Receive and DataArrival events.</returns>
        /// <remarks>This only runs when legacy support is off.</remarks>
        private FileData HandleIncomingFile(FileData data)
        {
            data.Info = new FileInfo(Path.GetTempFileName());
            IncomingFiles.Add(data.Guid, data);
            return data;
        }

        /// <summary>
        /// Handles the data of an incoming file, but putting it into the temporary file already
        /// obtained through the other HandleIncomingFile method.
        /// </summary>
        /// <param name="part">The <see cref="FileDataPart"/> containing the data that was received.</param>
        /// <returns>The <see cref="FileData"/> object for the part that was received (for event raising).</returns>
        /// <remarks>This only runs when legacy support is off.</remarks>
        private FileData HandleIncomingFile(FileDataPart part)
        {
            if (!IncomingFiles.ContainsKey(part.FileGuid))
                throw new InvalidOperationException("File part received before file details.");

            var file = IncomingFiles[part.FileGuid];
            file.ReceivePart(file.Info, part);
            return file;
        }

        /// <summary>
        /// Listens for incoming connection requests on its own thread.
        /// </summary>
        private async void ListenTCP()
        {
            Parent.ChangeState(State.Listening);
            socket.Listen(Parent.MaxPendingConnections);
            while (Parent.State == State.Listening)
            {
                try
                {
                    Socket client = await socket.AcceptAsync();
                    if (client != null)
                    {
                        var args = Parent.OnConnectionRequest(Parent, new ConnectionRequestEventArgs(client));
                        if (args.Cancel)
                        {
                            args.Client.Disconnect(false);
                            args.Client.Close();
                        }
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    if (Parent.State != State.Closed)
                    {
                        Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                    Close();
                }
            }
        }

        /// <summary>
        /// Listens for incoming UDP data in its own thread - no connections required.
        /// </summary>
        private async void ListenUDP()
        {
            var ip = Socket.OSSupportsIPv6 ? IPAddress.IPv6Any : IPAddress.Any;
            IPEndPoint endPoint = new IPEndPoint(ip, 0);
            Parent.ChangeState(State.Listening);
            while (Parent.State == State.Listening)
            {
                byte[] buffer = new byte[Parent.BufferSize];
                try
                {
                    int receivedSize = await socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint);
                    if (receivedSize > 0)
                        StartProcessIncoming(buffer, receivedSize, endPoint);
                }
                catch (Exception ex)
                {
                    Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                    Close();
                }
            }
        }

        /// <summary>
        /// Processes incoming data and attempts to recreate the objects that were sent.
        /// Then raises the ReceiveProgress and/or DataArrival events.
        /// Runs in it's own thread to allow receiving to continue unhindered.
        /// </summary>
        private void ProcessIncoming()
        {
            ReceivedPacket packet = null;
            while (true)
            {
                // First lets get the incoming packet we are dealing with
                packet = null;
                lock (ReceivedPackets.SyncRoot)
                {
                    if (ReceivedPackets.Count < 1)
                    {
                        IsProcessingIncomingData = false;
                        return;
                    }
                    packet = ReceivedPackets.PopFront();
                }

                if (Parent.LegacySupport)
                {
                    // Legacy support is enabled, so just alert that it arrived
                    lock (((ICollection)ReceivedBuffer).SyncRoot)
                        ReceivedBuffer.Enqueue(packet.Data);
                    Parent.OnDataArrival(Parent, new DataArrivalEventArgs(packet.Data.LongLength, packet.RemoteEndPoint));
                    continue;
                }

                // Let's get the header
                byte[] data = packet.Data;
                bool throwLegacyError = false;
                while (!Header.Completed && (packet.Data != null & packet.Data.Length > 0))
                {
                    if (!Header.ProcessHeader(ref data, ref ProcessingByteBuffer))
                    {
                        throwLegacyError = true;
                        break;
                    }
                }

                // Check for header error
                if (throwLegacyError || (!Header.Completed && ProcessingByteBuffer.Count > 10))
                {
                    Parent.OnErrorReceived(Parent, ErrorReceivedEventArgs.Create(new WinsockException("Unable to determine the size of the incoming packet. You may need to turn on Legacy Support.")));
                    Close();
                    break;
                }

                int receivedSize = data.Length;
                if (Header.Completed && ProcessingByteBuffer.Count + data.Length >= Header.Size)
                {
                    // We have the full object that was sent
                    data = ProcessingByteBuffer.Combine(data);
                    ProcessingByteBuffer.Clear();

                    byte[] objectData = null;
                    if (data.Length > Header.Size)
                    {
                        // There is extra data here - get only what we need
                        // then push the rest back on the queue
                        objectData = ArrayMethods.Shrink(ref data, Header.Size);
                        packet.Data = data;
                        lock (ReceivedPackets.SyncRoot)
                            ReceivedPackets.PushFront(packet);
                    }
                    else
                        objectData = data;

                    // Try converting the bytes back to the object.
                    var receivedObject = ObjectPacker.Unpack(objectData);
                    var receivedType = receivedObject.GetType();
                    if (receivedType == typeof(FileData) || receivedType == typeof(FileDataPart))
                    {
                        // Looks like we are dealing with an incoming file
                        // Handle the data and get a reference to the incoming file
                        FileData file = null;
                        try
                        {
                            file = (receivedType == typeof(FileData)) ?
                                HandleIncomingFile((FileData)receivedObject) :
                                HandleIncomingFile((FileDataPart)receivedObject);
                        }
                        catch (Exception ex)
                        {
                            Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                            Close();
                            break;
                        }

                        // This part of the file is done, so we can 
                        // reset the header for the next object
                        Header.Reset();
                        if (file != null)
                        {
                            // We've got the file, raise the events
                            // and remove our in progress reference to the file
                            Parent.OnReceiveProgress(Parent, new ReceiveProgressEventArgs(file.LastReceivedSize, file.ReceivedBytes, file.FileSize, packet.RemoteEndPoint));
                            if (file.ReceiveCompleted)
                            {
                                IncomingFiles.Remove(file.Guid);
                                lock (((ICollection)ReceivedBuffer).SyncRoot)
                                    ReceivedBuffer.Enqueue(file);
                                Parent.OnDataArrival(Parent, new DataArrivalEventArgs(file.FileSize, packet.RemoteEndPoint));
                            }
                        }
                    }
                    else
                    {
                        // Incoming object was not a file (could be a byte[])
                        // Store it in the queue and raise the events
                        lock (((ICollection)ReceivedBuffer).SyncRoot)
                            ReceivedBuffer.Enqueue(receivedObject);

                        Parent.OnReceiveProgress(Parent, new ReceiveProgressEventArgs(receivedSize, objectData.Length, Header.Size, packet.RemoteEndPoint));
                        Header.Reset();
                        Parent.OnDataArrival(Parent, new DataArrivalEventArgs(objectData.Length, packet.RemoteEndPoint));
                    }
                }
                else
                {
                    // Either the header wasn't completed, or we haven't got 
                    // all of the object yet, either way we need more data
                    // store what we've got into a temporary buffer
                    ProcessingByteBuffer.Add(data);
                    Parent.OnReceiveProgress(Parent, new ReceiveProgressEventArgs(receivedSize, ProcessingByteBuffer.Count, Header.Size, packet.RemoteEndPoint));
                }
            }

            // Exit the processing thread, and allow another one to be created
            lock (ReceivedPackets.SyncRoot)
                IsProcessingIncomingData = false;
        }

        private X509Certificate SelectUserCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return Parent.Certificate;
        }

        private async void SendLoop()
        {
            SendPacket packet = null;
            while(Parent.State == State.Connected)
            {
                packet = null;
                lock (SendBuffer.SyncRoot)
                {
                    if(SendBuffer.Count == 0)
                    {
                        IsSending = false;
                        return;
                    }
                    packet = SendBuffer.PopFront();
                }

                byte[] dataToSend = null;
                try
                {
                    dataToSend = packet.GetDataToSend(Parent.BufferSize, Parent.LegacySupport);
                }
                catch (Exception ex)
                {
                    Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                    break;
                }

                if (dataToSend == null)
                    continue;

                try
                {
                    if(Parent.Protocol == Protocol.Tcp)
                    {
                        await socketStream.WriteAsync(dataToSend, 0, dataToSend.Length);
                        packet.TotalSent += dataToSend.Length;
                    }
                    else // UDP
                    {
                        packet.TotalSent += await socket.SendToAsync(dataToSend, 0, dataToSend.Length, SocketFlags.None, packet.Destination);
                    }
                }
                catch(Exception ex)
                {
                    Parent.OnErrorReceived(Parent, ex.AsEventArgs());
                    break;
                }

                if (!packet.SendCompleted)
                {
                    lock (SendBuffer.SyncRoot)
                        SendBuffer.PushFront(packet);
                }
                Parent.OnSendProgress(Parent, new SendProgressEventArgs(packet, dataToSend.Length));
            }
            lock(SendBuffer.SyncRoot)
                IsSending = false;
        }

        /// <summary>
        /// Starts the processing of incoming data.
        /// Processing will happen in it's own thread to avoid having processing holding up the receiving process.
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array containing the data that was received.</param>
        /// <param name="size">The size of the data received.</param>
        /// <param name="remote">The <see cref="IPEndPoint"/> of the remote machine.</param>
        private void StartProcessIncoming(byte[] data, int size, IPEndPoint remote)
        {
            if (size <= 0) return;
            lock (ReceivedPackets.SyncRoot)
            {
                ReceivedPackets.PushBack(new ReceivedPacket(data, remote));

                if (IsProcessingIncomingData) return;
                IsProcessingIncomingData = true;
                (new Thread(ProcessIncoming)).Start();
            }
        }

        /// <summary>
        /// Starts the sending thread - assuming it's not already running
        /// </summary>
        private void StartSendLoop()
        {
            lock (SendBuffer.SyncRoot)
            {
                if (!IsSending)
                {
                    IsSending = true;
                    (new Thread(SendLoop)).Start();
                }
            }
        }

        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var args = Parent.OnValidateCertificate(Parent, new ValidateCertificateEventArgs(null, certificate, chain, sslPolicyErrors));
            return args.IsValid;
        }
        
        #endregion
    }

    public class ValidateCertificateEventArgs : RemoteEndPointEventArgsBase
    {
        public ValidateCertificateEventArgs(IPEndPoint remoteEP, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            RemoteEndPoint = remoteEP;
            Certificate = certificate;
            CertificateChain = chain;
            SslPolicyErrors = sslPolicyErrors;
            IsValid = (sslPolicyErrors == SslPolicyErrors.None);
        }

        public X509Certificate Certificate { get; private set; }
        public X509Chain CertificateChain { get; private set; }
        public SslPolicyErrors SslPolicyErrors { get; private set; }
        public bool IsValid { get; set; }
    }

    
}
