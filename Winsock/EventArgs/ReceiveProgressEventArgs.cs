using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Provides data for the <see cref="Winsock"/>.ReceiveProgress event.
    /// </summary>
    public class ReceiveProgressEventArgs : RemoteEndPointEventArgsBase
    {
        public ReceiveProgressEventArgs(int bytesReceived, long totalBytesReceived, long bytesTotal, IPEndPoint remoteEndPoint)
        {
            BytesReceived = bytesReceived;
            TotalBytesReceived = totalBytesReceived;
            TotalBytes = bytesTotal;
            RemoteEndPoint = remoteEndPoint;
        }

        /// <summary>
        /// Gets the number of bytes received since the last <see cref="Winsock"/>.ReceiveProgress event.
        /// </summary>
        public int BytesReceived { get; private set; }

        /// <summary>
        /// Gets the total number of bytes received for the incoming object.
        /// </summary>
        public long TotalBytesReceived { get; private set; }

        /// <summary>
        /// Gets the total number of bytes the incoming object has.
        /// </summary>
        public long TotalBytes { get; private set; }

        /// <summary>
        /// Gets the percentage of the incoming object that has been received (0 to 1).
        /// </summary>
        public decimal PercentComplete { get { return TotalBytesReceived / (decimal)TotalBytes; } }

        /// <summary>
        /// Gets a boolean value indicating whether the incoming object has been fully received.
        /// </summary>
        public bool Completed { get { return (TotalBytes == TotalBytesReceived); } }
    }
}
