using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Provides information about the incoming packet while it is stored in a buffer awaiting processing.
    /// </summary>
    internal class ReceivedPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedPacket"/> class.
        /// </summary>
        /// <param name="data">The data that was received.</param>
        /// <param name="endPoint">Where the data was received from.</param>
        public ReceivedPacket(byte[] data, IPEndPoint endPoint)
        {
            Data = data;
            RemoteEndPoint = endPoint;
        }

        /// <summary>
        /// Gets or sets the data that was received.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the remote endpoint where the data was received from.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; }
    }
}
