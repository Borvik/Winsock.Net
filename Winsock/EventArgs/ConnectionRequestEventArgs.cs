using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Provides data for the <see cref="Winsock"/>.ConnectionRequest event.
    /// </summary>
    public class ConnectionRequestEventArgs : RemoteEndPointEventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestEventArgs"/> class with the specified client.
        /// </summary>
        /// <param name="client">A <see cref="Socket"/> object representing the incoming connection request.</param>
        public ConnectionRequestEventArgs(Socket client) : this(client, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestEventArgs"/> class with the specified client.
        /// </summary>
        /// <param name="client">A <see cref="Socket"/> object representing the incoming connection request.</param>
        /// <param name="cancel">true to cancel the event; otherwise false.</param>
        public ConnectionRequestEventArgs(Socket client, bool cancel)
        {
            Client = client;
            RemoteEndPoint = Client?.RemoteEndPoint as IPEndPoint;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// A <see cref="Socket"/> containing the connection information of the incoming connection request.
        /// </summary>
        public Socket Client { get; private set; }
    }
}
