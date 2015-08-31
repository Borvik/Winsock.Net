using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// 
    /// </summary>
    internal class AsyncSocket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocket"/> class using the specified address family, socket type and protocol.
        /// </summary>
        /// <param name="addressFamily">One of the <see cref="AddressFamily"/> values.</param>
        /// <param name="socketType">One of the <see cref="SocketType"/> values.</param>
        /// <param name="protocolType">One of the <see cref="ProtocolType"/> values.</param>
        public AsyncSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            internalSocket = new Socket(addressFamily, socketType, protocolType);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocket"/> class using the specified socket.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to create the <see cref="AsyncSocket"/> from.</param>
        public AsyncSocket(Socket socket) { internalSocket = socket; }

        internal Socket internalSocket;
        private object _syncRoot;
        private object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        internal void Close() { internalSocket.Close(); }
        internal void Close(int timeout) { internalSocket.Close(timeout); }

        internal void Shutdown(SocketShutdown how) { internalSocket.Shutdown(how); }

        internal void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            internalSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }
        internal void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            internalSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        internal void Bind(IPEndPoint localEndpoint)
        {
            internalSocket.Bind(localEndpoint);
        }

        internal void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            internalSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }
        internal void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            internalSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        internal void Listen(int maxPendingConnections) { internalSocket.Listen(maxPendingConnections); }



        /// <summary>
        /// Gets a value to indicate if the <see cref="AsyncSocket"/> is still connected to the remote machine.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    return internalSocket != null && !(internalSocket.Poll(-1, SelectMode.SelectRead) && internalSocket.Available == 0);
                }
                catch (ObjectDisposedException) { return false; }
            }
        }

        public EndPoint LocalEndPoint { get { return internalSocket.LocalEndPoint; } }
        public EndPoint RemoteEndPoint { get { return internalSocket.RemoteEndPoint; } }

        public Task<Socket> AcceptAsync()
        {
            return Task<Socket>.Factory.FromAsync(internalSocket.BeginAccept, internalSocket.EndAccept, null);
        }
        public Task ConnectAsync(EndPoint endPoint)
        {
            return Task.Factory.FromAsync(internalSocket.BeginConnect, internalSocket.EndConnect, endPoint, null);
        }
        public Task<int> ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref IPEndPoint remoteEP)
        {
            return Task<int>.Factory.FromAsync(
                (c, s) =>
                {
                    EndPoint endPoint = (EndPoint)s;
                    return internalSocket.BeginReceiveFrom(buffer, offset, size, socketFlags, ref endPoint, c, s);
                },
                (ia) =>
                {
                    EndPoint endPoint = (EndPoint)ia.AsyncState;
                    return internalSocket.EndReceiveFrom(ia, ref endPoint);
                },
                remoteEP);
        }
        
        public Task<int> SendToAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return Task<int>.Factory.FromAsync(
                (c, s) =>
                {
                    return internalSocket.BeginSendTo(buffer, offset, size, socketFlags, remoteEP, c, s);
                },
                (ia) =>
                {
                    return internalSocket.EndSendTo(ia);
                },
                null);
        }
    }
}
