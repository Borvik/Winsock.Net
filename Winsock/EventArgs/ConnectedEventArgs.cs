using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Provides data for the <see cref="Winsock"/>.Connected event.
    /// </summary>
    public class ConnectedEventArgs : RemoteEndPointEventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedEventArgs"/> with the specified remote endpoint.
        /// </summary>
        /// <param name="endPoint">An <see cref="IPEndPoint"/> representing the endpoint of the remote device.</param>
        public ConnectedEventArgs(IPEndPoint endPoint)
        {
            RemoteEndPoint = endPoint;
        }
    }
}
