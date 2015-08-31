using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Enumeration containing the various supported network protocols.
    /// </summary>
    public enum Protocol
    {
        /// <summary>
        /// Transmission Control Protocol - a connection oriented protocol.
        /// </summary>
        Tcp = 0,
        /// <summary>
        /// User Datagram Protocol - a connection-less protocol.
        /// </summary>
        Udp = 1
    }
}
