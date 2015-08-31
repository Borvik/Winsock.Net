using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    //public class WinsockStream : NetworkStream
    //{
    //    public WinsockStream(Socket socket) : base(socket) { }
    //    public WinsockStream(Socket socket, bool ownsSocket) : base(socket, ownsSocket) { }
    //    public WinsockStream(Socket socket, FileAccess access) : base(socket, access) { }
    //    public WinsockStream(Socket socket, FileAccess access, bool ownsSocket) : base(socket, access, ownsSocket) { }

    //    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    //    {
    //        //var task = new WinsockStreamReadWriteTask<int>(null);
    //        /*
    //        return Task<int>.Factory.FromAsync(
    //            (c, s) =>
    //            {
    //                return internalSocket.BeginSendTo(buffer, offset, size, socketFlags, remoteEP, c, s);
    //            },
    //            (ia) =>
    //            {
    //                return internalSocket.EndSendTo(ia);
    //            },
    //            null);
    //        */
    //        ManualResetEvent readDone = new ManualResetEvent(false);
    //        //Task<int>.Factory.FromAsync()
    //        //return base.ReadAsync(buffer, offset, count, cancellationToken);
    //        var task = Task<int>.Factory.FromAsync(
    //            (c, s) => 
    //            {
    //                return base.BeginRead(buffer, offset, count, c, s);
    //            },
    //            (ia) => 
    //            {
    //                var result = base.EndRead(ia);
    //                return result;
    //            },
    //            readDone);
    //        return task;
    //    }

        
    //    private sealed class WinsockStreamReadWriteTask<T> : Task<T>
    //    {
    //        internal IAsyncResult _asyncResult;
    //        internal CancellationToken _cancellationToken;
    //        internal CancellationTokenRegistration _registration;

    //        internal WinsockStreamReadWriteTask(CancellationToken cancellationToken) : base()
    //        {

    //        }
    //    }
    //}
}
