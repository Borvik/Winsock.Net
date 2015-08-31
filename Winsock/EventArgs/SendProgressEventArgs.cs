using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    public class SendProgressEventArgs : RemoteEndPointEventArgsBase
    {
        internal SendProgressEventArgs(SendPacket packet, int bytesSent)
        {
            RemoteEndPoint = packet.Destination;
            TotalBytes = packet.TotalLength;
            TotalSent = packet.TotalSent;
            BytesSent = bytesSent;
        }

        /// <summary>
        /// Gets the total number of bytes the object being sent has.
        /// </summary>
        public long TotalBytes { get; private set; }

        /// <summary>
        /// Gets the total number of bytes of the object that has been sent so far.
        /// </summary>
        public long TotalSent { get; private set; }

        /// <summary>
        /// Gets the number of bytes that has been sent, since the last <see cref="Winsock"/>.SendProgress event.
        /// </summary>
        public int BytesSent { get; private set; }

        /// <summary>
        /// Gets the percentage of the outgoing object that has been sent (0 to 1).
        /// </summary>
        public decimal PercentComplete { get { return TotalSent / (decimal)TotalBytes; } }

        /// <summary>
        /// Gets a boolean value indicating whether the outgoing object has been fully sent.
        /// </summary>
        public bool Completed { get { return (TotalSent == TotalBytes); } }
    }
}
