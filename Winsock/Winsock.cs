using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Treorisoft.Net.Utilities;

namespace Treorisoft.Net
{
    [ToolboxBitmap(typeof(Winsock), "socket.png")]
    public class Winsock : Component, ICustomTypeDescriptor
    {
        public Winsock()
        {
            _socket = new WinSocket(this);
            _clients = new WinsockCollection(this);
        }

        #region Constants

        private const int DEFAULT_BUFFER_SIZE = 8192;
        private const bool DEFAULT_LEGACY_SUPPORT = false;
        private const int DEFAULT_LOCAL_PORT = 8080;
        private const int DEFAULT_MAX_PENDING = 1;
        private const Protocol DEFAULT_PROTOCOL = Protocol.Tcp;
        private const string DEFAULT_REMOTE_HOST = "localhost";
        private const int DEFAULT_REMOTE_PORT = 8080;
        private const SslProtocols DEFAULT_SSL_PROTOCOL = SslProtocols.None;
        private const TextEncoding DEFAULT_TEXT_ENCODING = TextEncoding.Default;

        #endregion

        #region Fields

        private int _bufferSize = DEFAULT_BUFFER_SIZE;
        private X509Certificate _certificate = null;
        private WinsockCollection _clients = null;
        private Encoding _customTextEncoding = Encoding.Default;
        private bool _legacySupport = DEFAULT_LEGACY_SUPPORT;
        private int _localPort = DEFAULT_LOCAL_PORT;
        private int _maxPendingConnections = DEFAULT_MAX_PENDING;
        private Protocol _protocol = DEFAULT_PROTOCOL;
        private string _remoteHost = DEFAULT_REMOTE_HOST;
        private int _remotePort = DEFAULT_REMOTE_PORT;
        private SslProtocols _sslProtocol = DEFAULT_SSL_PROTOCOL;
        private State _state = State.Closed;
        private TextEncoding _textEncoding = DEFAULT_TEXT_ENCODING;

        private bool IsDisposed = false;
        private WinSocket _socket = null;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the value of the <see cref="BufferSize"/> property has changed.
        /// </summary>
        public event EventHandler BufferSizeChanged;
        /// <summary>
        /// Occurs when the value of the <see cref="Certificate"/> property has changed.
        /// </summary>
        public event EventHandler CertificateChanged;
        /// <summary>
        /// Occurs when a client has been added or removed from the <see cref="Clients"/> collection.
        /// </summary>
        public event EventHandler ClientCountChanged;
        /// <summary>
        /// Occurs on the client/server when a connection has been established.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;
        /// <summary>
        /// Occurs on the server when a client is attempting to connect.
        /// </summary>
        public event EventHandler<ConnectionRequestEventArgs> ConnectionRequest;
        /// <summary>
        /// Occurs when the value of the <see cref="CustomTextEncoding"/> property has changed.
        /// </summary>
        public event EventHandler CustomTextEncodingChanged;
        /// <summary>
        /// Occurs when data arrives from the connected device.
        /// </summary>
        public event EventHandler<DataArrivalEventArgs> DataArrival;
        /// <summary>
        /// Occurs when a connected socket is closed.
        /// </summary>
        public event EventHandler Disconnected;
        /// <summary>
        /// Occurs when an internal error is detected.
        /// </summary>
        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
        /// <summary>
        /// Occurs when the value of the <see cref="LegacySupport"/> property has changed.
        /// </summary>
        public event EventHandler LegacySupportChanged;
        /// <summary>
        /// Occurs when the value of the <see cref="LocalPort"/> property has changed.
        /// </summary>
        public event EventHandler LocalPortChanged;
        /// <summary>
        /// Occurs when the value of the <see cref="MaxPendingConnections"/> property has changed.
        /// </summary>
        public event EventHandler MaxPendingConnectionsChanged;
        /// <summary>
        /// Occurs when the value of the <see cref="Protocol"/> property has changed.
        /// </summary>
        public event EventHandler ProtocolChanged;
        /// <summary>
        /// Occurs when not using legacy support and a portion of the data arrives.
        /// </summary>
        public event EventHandler<ReceiveProgressEventArgs> ReceiveProgress;
        /// <summary>
        /// Occurs when the value of the <see cref="RemoteHost"/> property has changed;
        /// </summary>
        public event EventHandler RemoteHostChanged;
        /// <summary>
        /// Occurs when the value of the <see cref="RemotePort"/> property has changed.
        /// </summary>
        public event EventHandler RemotePortChanged;
        /// <summary>
        /// Occurs when data is sent over the network (at intervals of <see cref="BufferSize"/>).
        /// </summary>
        public event EventHandler<SendProgressEventArgs> SendProgress;
        /// <summary>
        /// Occurs when the value of the <see cref="SslProtocol"/> property has changed.
        /// </summary>
        public event EventHandler SslProtocolChanged;
        /// <summary>
        /// Occurs when the value of the <see cref="State"/> property has changed.
        /// </summary>
        public event EventHandler<StateChangedEventArgs> StateChanged;
        /// <summary>
        /// Occurs when the value of the <see cref="TextEncoding"/> property has changed.
        /// </summary>
        public event EventHandler TextEncodingChanged;
        /// <summary>
        /// Occurs when the client validates the server certificate, or a server validates a client certificate.
        /// </summary>
        public event EventHandler<ValidateCertificateEventArgs> ValidateCertificate;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the size of the buffer used to handle the transferred bytes.
        /// </summary>
        [DefaultValue(DEFAULT_BUFFER_SIZE), Category("Behavior"), Description("The size of the buffer used to handle the transferred bytes.")]
        public int BufferSize
        {
            get { return _bufferSize; }
            set
            {
                if (_bufferSize != value)
                {
                    _bufferSize = value;
                    OnBufferSizeChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SSL certificate that should be used to authenticate.
        /// </summary>
        [Editor(typeof(Designers.X509CertificateUIEditor), typeof(UITypeEditor)),
         TypeConverter(typeof(Designers.X509CertificateConverter)),
         AmbientValue(null),
         DefaultValue(null), Category("Security"), Description("The SSL certificate this server should use to authenticate itself as.")]
        public X509Certificate Certificate
        {
            get { return _certificate; }
            set
            {
                if (_certificate != value)
                {
                    if (State != State.Closed)
                        throw new WinsockException("Cannot change the server certificate while listening or connected to a remote computer.");

                    _certificate = value;
                    OnCertificateChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets the list of the connected clients.
        /// </summary>
        public WinsockCollection Clients { get { return _clients; } }

        /// <summary>
        /// Gets or sets the Encoding that should be used on strings when LegacySupport is enabled and TextEncoding is set to Custom.
        /// </summary>
        [Browsable(false)]
        public Encoding CustomTextEncoding
        {
            get { return _customTextEncoding; }
            set
            {
                if (_customTextEncoding != value)
                {
                    _customTextEncoding = value;
                    OnCustomTextEncodingChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether to enable legacy support for communicating with clients or servers that do not use this component.
        /// </summary>
        [DefaultValue(DEFAULT_LEGACY_SUPPORT), Category("Behavior"), Description("Indicates whether to enable legacy support for communicating with clients or servers that do not use this component.")]
        public bool LegacySupport
        {
            get { return _legacySupport; }
            set
            {
                if (_legacySupport != value)
                {
                    if (!value && Protocol == Protocol.Udp)
                        throw new WinsockException("LegacySupport is required for UDP connections.");

                    _legacySupport = value;
                    OnLegacySupportChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the local port on this server that other clients should connect to.
        /// </summary>
        [DefaultValue(DEFAULT_LOCAL_PORT), Category("Connection"), Description("The local port on this server that other clients should connect to.")]
        public int LocalPort
        {
            get { return _localPort; }
            set
            {
                if (_localPort != value)
                {
                    if (State != State.Closed && State != State.Closing)
                        throw new WinsockException("Cannot change the local port while Winsock is not closed.");

                    _localPort = value;
                    OnLocalPortChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of incoming connections that can be queued for acceptance.
        /// </summary>
        [DefaultValue(DEFAULT_MAX_PENDING), Category("Behavior"), Description("The maximum number of incoming connections that can be queued for acceptance.")]
        public int MaxPendingConnections
        {
            get { return _maxPendingConnections; }
            set
            {
                if (_maxPendingConnections != value)
                {
                    if (State == State.Listening)
                        throw new WinsockException("Cannot change the max pending connections count while already listening.");

                    _maxPendingConnections = value;
                    OnMaxPendingConnectionsChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the protocol that the client/server should use to communicate with.
        /// </summary>
        [DefaultValue(DEFAULT_PROTOCOL), Category("Connection"), Description("The protocol that the client/server should use to communicate with.")]
        public Protocol Protocol
        {
            get { return _protocol; }
            set
            {
                if (_protocol != value)
                {
                    if (State != State.Closed)
                        throw new WinsockException("Cannot change the protocol while listening or connected to a remote computer.");

                    _protocol = value;
                    OnProtocolChanged(this);
                    if (value == Protocol.Udp)
                        LegacySupport = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the hostname or IP address of the remote server this client should connect to.
        /// </summary>
        [DefaultValue(DEFAULT_REMOTE_HOST), Category("Connection"), Description("The hostname or IP address of the remote server this client should connect to.")]
        public string RemoteHost
        {
            get { return _remoteHost; }
            set
            {
                if (_remoteHost != value)
                {
                    if (State != State.Closed && State != State.Listening)
                        throw new WinsockException("Cannot change the remote host while already connected to a remote computer.");

                    _remoteHost = value;
                    OnRemoteHostChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or set the port on the remote server that this client should connect to.
        /// </summary>
        [DefaultValue(DEFAULT_REMOTE_PORT), Category("Connection"), Description("The port on the remote server that this client should connect to.")]
        public int RemotePort
        {
            get { return _remotePort; }
            set
            {
                if (_remotePort != value)
                {
                    if (State != State.Closed && State != State.Listening)
                        throw new WinsockException("Cannot change the remote port while already connected to a remote computer.");

                    _remotePort = value;
                    OnRemotePortChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SSL protocol that should be used when connecting to the remote client/server.
        /// </summary>
        [DefaultValue(DEFAULT_SSL_PROTOCOL), Category("Security"), Description("The SSL protocol that should be used when connecting to the remote client/server.")]
        public SslProtocols SslProtocol
        {
            get { return _sslProtocol; }
            set
            {
                if (_sslProtocol != value)
                {
                    if (State != State.Closed)
                        throw new WinsockException("Cannot change the ssl protocol while listening or connected to a remote computer.");

                    _sslProtocol = value;
                    OnSslProtocolChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets the current state of the Winsock control.
        /// </summary>
        public State State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets or sets the encoding that should be used when encoding strings when LegacySupport is turned on.
        /// </summary>
        [DefaultValue(DEFAULT_TEXT_ENCODING), Category("Behavior"), Description("The encoding that should be used when encoding strings when LegacySupport is turned on.")]
        public TextEncoding TextEncoding
        {
            get { return _textEncoding; }
            set
            {
                if (_textEncoding != value)
                {
                    _textEncoding = value;
                    OnTextEncodingChanged(this);
                }
            }
        }

        #endregion

        #region Public Methods

        #region Server Methods

        /// <summary>
        /// Accepts an incoming connection request and begins to monitor it for incoming data. 
        /// </summary>
        /// <param name="client">A <see cref="Socket" /> that represents the client being accepted.</param>
        public void Accept(Socket client)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            var w = new Winsock()
            {
                Protocol = Protocol,
                LegacySupport = LegacySupport,
                BufferSize = _bufferSize,
                TextEncoding = _textEncoding,
                CustomTextEncoding = _customTextEncoding,
                SslProtocol = _sslProtocol,
            };

            // We add it to the clients list so any event handlers can fire.
            var guid = Clients.Add(w);
            if (w._socket.Accept(client))
            {
                w.ChangeLocalPort(w._socket.LocalEndPoint.Port);
                w.ChangeRemoteHost(w._socket.RemoteEndPoint.Address.ToString());
                w.ChangeRemotePort(w._socket.RemoteEndPoint.Port);
            }
            else
                Clients.Remove(guid);
        }

        /// <summary>
        /// Places the <see cref="Winsock"/> into a listening state.
        /// </summary>
        public void Listen()
        {
            Listen(null, LocalPort, Certificate);
        }
        /// <summary>
        /// Places the <see cref="Winsock"/> into a listening state.
        /// </summary>
        /// <param name="port">The port the <see cref="Winsock"/> should listen on.</param>
        public void Listen(int port)
        {
            Listen(null, port, Certificate);
        }
        /// <summary>
        /// Places the <see cref="Winsock"/> into a listening state.
        /// </summary>
        /// <param name="port">The port the <see cref="Winsock"/> should listen on.</param>
        /// <param name="x509CertificatePath">The path to the SSL certificate the server should to use authenticate itself as.</param>
        public void Listen(int port, string x509CertificatePath)
        {
            Listen(null, port, x509CertificatePath);
        }
        /// <summary>
        /// Places the <see cref="Winsock"/> into a listening state.
        /// </summary>
        /// <param name="ipAddress">The IP address the server should listen on.</param>
        public void Listen(string ipAddress)
        {
            Listen(ipAddress, LocalPort, Certificate);
        }
        /// <summary>
        /// Places the <see cref="Winsock"/> into a listening state.
        /// </summary>
        /// <param name="ipAddress">The IP address the server should listen on.</param>
        /// <param name="x509CertificatePath">The path to the SSL certificate the server should to use authenticate itself as.</param>
        public void Listen(string ipAddress, string x509CertificatePath)
        {
            Listen(ipAddress, LocalPort, x509CertificatePath);
        }
        /// <summary>
        /// Places the <see cref="Winsock"/> into a listening state.
        /// </summary>
        /// <param name="ipAddress">The IP address the server should listen on.</param>
        /// <param name="port">The port the <see cref="Winsock"/> should listen on.</param>
        public void Listen(string ipAddress, int port)
        {
            Listen(ipAddress, port, Certificate);
        }
        /// <summary>
        /// Places the <see cref="Winsock"/> into a listening state.
        /// </summary>
        /// <param name="ipAddress">The IP address the server should listen on.</param>
        /// <param name="port">The port the <see cref="Winsock"/> should listen on.</param>
        /// <param name="x509CertificatePath">The path to the SSL certificate the server should use to authenticate itself as.</param>
        public void Listen(string ipAddress, int port, string x509CertificatePath)
        {
            var certificate = X509Certificate.CreateFromCertFile(x509CertificatePath);
            if(certificate == null)
                throw new WinsockException("Unable to load the certificate file - should be a valid PKCS7 signed X.509 certificate.");
            Listen(ipAddress, port, certificate);
        }
        /// <summary>
        /// Places the <see cref="Winsock"/> into a listening state.
        /// </summary>
        /// <param name="ipAddress">The IP address the server should listen on.</param>
        /// <param name="port">The port the <see cref="Winsock"/> should listen on.</param>
        /// <param name="certificate">A <see cref="X509Certificate"/> that the server should use to authenticatie itself as.</param>
        public void Listen(string ipAddress, int port, X509Certificate certificate)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (State != State.Closed) throw new WinsockException("Unable to start listening when the state is not Closed.");
            if (port < 0) throw new WinsockException("Cannot listen on a port less than zero.");
            if (certificate == null && SslProtocol != SslProtocols.None)
                throw new WinsockException("You must supply a SSL certificate when SslProtocol is not None.");

            IPAddress addr = null;
            if (ipAddress != null && !IPAddress.TryParse(ipAddress, out addr))
                throw new WinsockException("The IP address specified is not a valid IP address.");

            Certificate = certificate;
            _socket.Listen(addr, port);
        }

        #endregion

        #region Client Methods

        /// <summary>
        /// Establishes a connection to a remote host.
        /// </summary>
        public void Connect()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            _socket.Connect(RemoteHost, RemotePort);
        }
        /// <summary>
        /// Establishes a secure connection to a remote host.
        /// </summary>
        /// <param name="sslHost">The name of the host to validate the server certificate for.</param>
        public void Connect(string sslHost)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            _socket.Connect(RemoteHost, RemotePort, sslHost);
        }
        /// <summary>
        /// Establishes a secure connection to a remote host.
        /// </summary>
        /// <param name="sslHost">The name of the host to validate the server certificate for.</param>
        /// <param name="x509CertificatePath">The path to the SSL certificate the client should to use authenticate itself as.</param>
        public void Connect(string sslHost, string x509CertificatePath)
        {
            Connect(RemoteHost, RemotePort, sslHost, x509CertificatePath);
        }
        /// <summary>
        /// Establishes a secure connection to a remote host.
        /// </summary>
        /// <param name="sslHost">The name of the host to validate the server certificate for.</param>
        /// <param name="certificate">A <see cref="X509Certificate"/> that the client should use to authenticatie itself as.</param>
        public void Connect(string sslHost, X509Certificate certificate)
        {
            Connect(RemoteHost, RemotePort, sslHost, certificate);
        }
        /// <summary>
        /// Establishes a connection to a remote host.
        /// </summary>
        /// <param name="remoteHostOrIp">A value containing the Hostname or IP address of the remote host.</param>
        /// <param name="remotePort">A value indicating the port on the remote host to connect to.</param>
        public void Connect(string remoteHostOrIp, int remotePort)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            RemoteHost = remoteHostOrIp;
            RemotePort = remotePort;
            _socket.Connect(RemoteHost, RemotePort);
        }
        /// <summary>
        /// Establishes a secure connection to a remote host.
        /// </summary>
        /// <param name="remoteHostOrIp">A value containing the Hostname or IP address of the remote host.</param>
        /// <param name="remotePort">A value indicating the port on the remote host to connect to.</param>
        /// <param name="sslHost">The name of the host to validate the server certificate for.</param>
        public void Connect(string remoteHostOrIp, int remotePort, string sslHost)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            RemoteHost = remoteHostOrIp;
            RemotePort = remotePort;
            _socket.Connect(RemoteHost, RemotePort, sslHost);
        }
        /// <summary>
        /// Establishes a secure connection to a remote host.
        /// </summary>
        /// <param name="remoteHostOrIp">A value containing the Hostname or IP address of the remote host.</param>
        /// <param name="remotePort">A value indicating the port on the remote host to connect to.</param>
        /// <param name="sslHost">The name of the host to validate the server certificate for.</param>
        /// <param name="x509CertificatePath">The path to the SSL certificate the client should to use authenticate itself as.</param>
        public void Connect(string remoteHostOrIp, int remotePort, string sslHost, string x509CertificatePath)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            var certificate = X509Certificate.CreateFromCertFile(x509CertificatePath);
            if (certificate == null)
                throw new WinsockException("Unable to load the certificate file - should be a valid PKCS7 signed X.509 certificate.");

            Connect(remoteHostOrIp, remotePort, sslHost, certificate);
        }
        /// <summary>
        /// Establishes a secure connection to a remote host.
        /// </summary>
        /// <param name="remoteHostOrIp">A value containing the Hostname or IP address of the remote host.</param>
        /// <param name="remotePort">A value indicating the port on the remote host to connect to.</param>
        /// <param name="sslHost">The name of the host to validate the certificate for.</param>
        /// <param name="certificate">A <see cref="X509Certificate"/> that the client should use to authenticatie itself as.</param>
        public void Connect(string remoteHostOrIp, int remotePort, string sslHost, X509Certificate certificate)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (certificate == null && SslProtocol != SslProtocols.None)
                throw new WinsockException("You must supply a SSL certificate when SslProtocol is not None.");

            RemoteHost = remoteHostOrIp;
            RemotePort = remotePort;
            Certificate = certificate;
            _socket.Connect(remoteHostOrIp, remotePort, sslHost);
        }

        #endregion

        #region Client/Server Methods

        /// <summary>
        /// Closes and open or lisenting <see cref="Winsock" /> connection.
        /// </summary>
        public void Close() { _socket.Close(); }

        /// <summary>
        /// Returns the next object from the buffer, while also removing it from the buffer.
        /// </summary>
        /// <returns>A deserialized object converted to the data type you requested.</returns>
        public object Get()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            return _socket.Get();
        }
        /// <summary>
        /// Returns the next object from the buffer, while also removing it from the buffer.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> you wish to have the data returned as.</typeparam>
        /// <returns>A deserialized object converted to the data type you requested.</returns>
        public T Get<T>()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            return ConvertData<T>(_socket.Get());
        }

        /// <summary>
        /// Returns the next object from the buffer, preserving it's place in the buffer.
        /// </summary>
        /// <returns>A deserialized object converted to the data type you requested.</returns>
        public object Peek()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            return _socket.Peek();
        }
        /// <summary>
        /// Returns the next object from the buffer, preserving it's place in the buffer.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> you wish to have the data returned as.</typeparam>
        /// <returns>A deserialized object converted to the data type you requested.</returns>
        public T Peek<T>()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            return ConvertData<T>(_socket.Peek());
        }

        /// <summary>
        /// Returns the type of the next object in the buffer.
        /// </summary>
        /// <returns>The <see cref="Type"/> of the next item in the buffer.</returns>
        public Type PeekType() { return Peek()?.GetType(); }

        /// <summary>
        /// Sends an object to a connected socket on a remote computer.
        /// </summary>
        /// <param name="data">The <see cref="object"/> to be serialized and sent.</param>
        public void Send(object data)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            _socket.Send(ObjectPacker.Pack(data));
        }
        /// <summary>
        /// Sends a string to a connected socket on a remote computer.
        /// </summary>
        /// <param name="data">The <see cref="string"/> data to be sent.</param>
        public void Send(string data)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            _socket.Send(LegacySupport ? GetEncoding().GetBytes(data) : ObjectPacker.Pack(data));
        }
        /// <summary>
        /// Sends a sequence of bytes to a connected socket on a remote computer.
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array containing the data to be sent.</param>
        public void Send(byte[] data)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            _socket.Send(LegacySupport ? data : ObjectPacker.Pack(data));
        }

        /// <summary>
        /// Sends a file to a connected socket on a remote computer.
        /// </summary>
        /// <param name="fileName">The full path of the file to send.</param>
        public void SendFile(string fileName)
        {
            SendFile(new FileInfo(fileName));
        }
        /// <summary>
        /// Sends a file to a connected socket on a remote computer.
        /// </summary>
        /// <param name="fileInfo">A <see cref="FileInfo"/> object containing the information of the file to send.</param>
        public void SendFile(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            if (!fileInfo.Exists) throw new FileNotFoundException(fileInfo.FullName);
            _socket.Send(new FileData(fileInfo));
        }

        #endregion

        #endregion

        #region Private Methods

        private T ConvertData<T>(object data)
        {
            if (data == null) return default(T);
            if (data.GetType() == typeof(byte[]))
                return (T)((LegacySupport && typeof(T) == typeof(string)) ? GetEncoding().GetString((byte[])data) : ObjectPacker.Unpack((byte[])data));
            return (T)data;
        }

        internal Encoding GetEncoding()
        {
            switch (TextEncoding)
            {
                case TextEncoding.ASCII: return Encoding.ASCII;
                case TextEncoding.BigEndianUnicode: return Encoding.BigEndianUnicode;
                case TextEncoding.Unicode: return Encoding.Unicode;
                case TextEncoding.UTF32: return Encoding.UTF32;
                case TextEncoding.UTF7: return Encoding.UTF7;
                case TextEncoding.UTF8: return Encoding.UTF8;
                case TextEncoding.Custom:
                    return (CustomTextEncoding == null) ? Encoding.Default : CustomTextEncoding;
                default: return Encoding.Default;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            base.Dispose(disposing);
            if (disposing)
            {
                IsDisposed = true;
            }
        }


        internal void ChangeLocalPort(int port)
        {
            if(_localPort != port)
            {
                _localPort = port;
                OnLocalPortChanged(this);
            }
        }
        internal void ChangeRemoteHost(string host)
        {
            _remoteHost = host;
            OnRemoteHostChanged(this);
        }
        internal void ChangeRemotePort(int port)
        {
            _remotePort = port;
            OnRemotePortChanged(this);
        }
        internal void ChangeState(State state)
        {
            if(_state != state)
            {
                var args = new StateChangedEventArgs(_state, state);
                _state = state;
                OnStateChanged(this, args);
            }
        }

        internal void OnBufferSizeChanged(object sender) { BufferSizeChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnCertificateChanged(object sender) { CertificateChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnClientCountChanged(object sender) { ClientCountChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnConnected(IPEndPoint endPoint) { OnConnected(this, new ConnectedEventArgs(endPoint)); }
        internal void OnConnected(object sender, ConnectedEventArgs args) { Connected.SafeRaise(CanRaiseEvents, sender, args); }
        internal ConnectionRequestEventArgs OnConnectionRequest(object sender, ConnectionRequestEventArgs args) { ConnectionRequest.SafeRaise(CanRaiseEvents, sender, args); return args; }
        internal void OnCustomTextEncodingChanged(object sender) { CustomTextEncodingChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnDataArrival(object sender, DataArrivalEventArgs args) { DataArrival.SafeRaise(CanRaiseEvents, sender, args); }
        internal void OnDisconnected() { OnDisconnected(this, EventArgs.Empty); }
        internal void OnDisconnected(object sender, EventArgs args) { Disconnected.SafeRaise(CanRaiseEvents, sender, args); }
        internal void OnErrorReceived(object sender, ErrorReceivedEventArgs args) { ErrorReceived.SafeRaise(CanRaiseEvents, sender, args); }
        internal void OnLegacySupportChanged(object sender) { LegacySupportChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnLocalPortChanged(object sender) { LocalPortChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnMaxPendingConnectionsChanged(object sender) { MaxPendingConnectionsChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnProtocolChanged(object sender) { ProtocolChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnReceiveProgress(object sender, ReceiveProgressEventArgs args) { ReceiveProgress.SafeRaise(CanRaiseEvents, sender, args); }
        internal void OnRemoteHostChanged(object sender) { RemoteHostChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnRemotePortChanged(object sender) { RemotePortChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnSendProgress(object sender, SendProgressEventArgs args) { SendProgress.SafeRaise(CanRaiseEvents, sender, args); }
        internal void OnSslProtocolChanged(object sender) { SslProtocolChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal void OnStateChanged(object sender, StateChangedEventArgs args) { StateChanged.SafeRaise(CanRaiseEvents, sender, args); }
        internal void OnTextEncodingChanged(object sender) { TextEncodingChanged.SafeRaise(CanRaiseEvents, sender, EventArgs.Empty); }
        internal ValidateCertificateEventArgs OnValidateCertificate(object sender, ValidateCertificateEventArgs args) { ValidateCertificate.SafeRaise(CanRaiseEvents, sender, args); return args; }

        #endregion

        #region ICustomTypeDescriptor

        /// <summary>
        /// Returns a collection of attributes for the <see cref="Winsock"/> object.
        /// </summary>
        /// <returns>An <see cref="AttributeCollection"/> containing the attributes for this object.</returns>
        public AttributeCollection GetAttributes() { return TypeDescriptor.GetAttributes(this, true); }

        /// <summary>
        /// Returns the name of the class for the <see cref="Winsock"/> object.
        /// </summary>
        /// <returns>Winsock</returns>
        public string GetClassName() { return TypeDescriptor.GetClassName(this, true); }

        /// <summary>
        /// Returns the name of the <see cref="Winsock"/> object.
        /// </summary>
        /// <returns>The name of the <see cref="Winsock"/>, or null if the <see cref="Winsock"/> does not have a name.</returns>
        public string GetComponentName() { return TypeDescriptor.GetComponentName(this, true); }

        /// <summary>
        /// Returns the <see cref="TypeConverter"/> for the <see cref="Winsock"/> object.
        /// </summary>
        /// <returns>A <see cref="TypeConverter"/> that is the converter for this <see cref="Winsock"/>, or null if there is no <see cref="TypeConverter"/> set.</returns>
        public TypeConverter GetConverter() { return TypeDescriptor.GetConverter(this, true); }

        /// <summary>
        /// Returns the default event of the <see cref="Winsock"/> object.
        /// </summary>
        /// <returns>An <see cref="EventDescriptor"/> that represents the default event for this object, or null if this object does not have events.</returns>
        public EventDescriptor GetDefaultEvent() { return TypeDescriptor.GetDefaultEvent(this, true); }

        /// <summary>
        /// Returns the default property for this instance of the <see cref="Winsock"/>.
        /// </summary>
        /// <returns>A <see cref="PropertyDescriptor"/> that represents the default property for this object, or null if this object does not have properties.</returns>
        public PropertyDescriptor GetDefaultProperty() { return TypeDescriptor.GetDefaultProperty(this, true); }

        /// <summary>
        /// Returns an editor of the specified type for this instance of the <see cref="Winsock"/>.
        /// </summary>
        /// <param name="editorBaseType">A <see cref="Type"/> that represents the editor for this object.</param>
        /// <returns>An <see cref="object"/> of the specified type that is the editor for this <see cref="Winsock"/>, or null if the editor cannot be found.</returns>
        public object GetEditor(Type editorBaseType) { return TypeDescriptor.GetEditor(this, editorBaseType, true); }

        /// <summary>
        /// Returns the events for this instance of the <see cref="Winsock"/>.
        /// </summary>
        /// <returns>An <see cref="EventDescriptorCollection"/> that represents the events for this <see cref="Winsock"/> instance.</returns>
        public EventDescriptorCollection GetEvents() { return TypeDescriptor.GetEvents(this, true); }

        /// <summary>
        /// Returns the events for this instance of the <see cref="Winsock"/> using the specified attribute array as a filter.
        /// </summary>
        /// <param name="attributes">An array of type <see cref="Attribute"/> that is used as a filter.</param>
        /// <returns>An <see cref="EventDescriptorCollection"/> that represents the filtered events for this <see cref="Winsock"/> instance.</returns>
        public EventDescriptorCollection GetEvents(Attribute[] attributes) { return TypeDescriptor.GetEvents(this, attributes, true); }


        protected virtual PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection collection)
        {
            if (DesignMode)
            {
                PropertyDescriptor[] array = new PropertyDescriptor[collection.Count];
                collection.CopyTo(array, 0);
                collection = new PropertyDescriptorCollection(array);
                collection.Remove(collection["State"]);
            }
            return collection;
        }

        /// <summary>
        /// Returns the properties for this instance of the <see cref="Winsock"/>.
        /// </summary>
        /// <returns>A <see cref="PropertyDescriptorCollection"/> that represents the properties for this <see cref="Winsock"/> instance.</returns>
        public PropertyDescriptorCollection GetProperties()
        {
            return FilterProperties(TypeDescriptor.GetProperties(this, true));
        }

        /// <summary>
        /// Returns the properties for this <see cref="Winsock"/> instance using the attribute array as a filter.
        /// </summary>
        /// <param name="attributes">An array of type <see cref="Attribute"/> that is used as a filter.</param>
        /// <returns>A <see cref="PropertyDescriptorCollection"/> that represents the filtered properties for this <see cref="Winsock"/> instance.</returns>
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return FilterProperties(TypeDescriptor.GetProperties(this, attributes, true));
        }

        /// <summary>
        /// Returns an object that contains the property described by the specified property descriptor.
        /// </summary>
        /// <param name="pd">A <see cref="PropertyDescriptor"/> that represents the property whose owner is to be found.</param>
        /// <returns>An <see cref="object"/> that represents the owner of the specified property.</returns>
        public object GetPropertyOwner(PropertyDescriptor pd) { return this; }

        #endregion
    }
}
