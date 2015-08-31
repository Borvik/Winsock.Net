using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Enumeration containing the various Winsock states.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// The Winsock is closed.
        /// </summary>
        Closed = 0,
        /// <summary>
        /// The Winsock is listening (TCP for connections, UDP for data).
        /// </summary>
        Listening = 1,
        /// <summary>
        /// The Winsock is attempting to resolve the remote host.
        /// </summary>
        ResolvingHost = 2,
        /// <summary>
        /// The remote host as been resolved to an IP address.
        /// </summary>
        HostResolved = 3,
        /// <summary>
        /// The Winsock is attempting to connect to the remote host.
        /// </summary>
        Connecting = 4,
        /// <summary>
        /// The Winsock is connected to a remote source.
        /// </summary>
        Connected = 5,
        /// <summary>
        /// The Winsock is attempting to close the connection.
        /// </summary>
        Closing = 6
    }
}
