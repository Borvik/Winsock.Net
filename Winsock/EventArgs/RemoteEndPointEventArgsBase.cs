using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    public abstract class RemoteEndPointEventArgsBase : EventArgs
    {
        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; protected set; }

        /// <summary>
        /// Gets the ip address of the remote endpoint.
        /// </summary>
        public string RemoteIP
        {
            get
            {
                if (RemoteEndPoint == null) return null;
                if (RemoteEndPoint.Address.IsIPv4MappedToIPv6)
                {
                    return RemoteEndPoint.Address.MapToIPv4().ToString();
                }
                return RemoteEndPoint.Address.ToString();
            }
        }

        /// <summary>
        /// Gets the port number used by the remote endpoint.
        /// </summary>
        public int? RemotePort { get { return RemoteEndPoint?.Port; } }
    }
}
