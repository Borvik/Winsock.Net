using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    public class DataArrivalEventArgs : RemoteEndPointEventArgsBase
    {
        public DataArrivalEventArgs(long bytes, IPEndPoint remoteEndPoint)
        {
            TotalBytes = bytes;
            RemoteEndPoint = remoteEndPoint;
        }

        /// <summary>
        /// Gets the number of bytes received.
        /// </summary>
        public long TotalBytes { get; private set; }
    }
}
