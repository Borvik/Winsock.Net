using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Provides data for the <see cref="Winsock"/>.ErrorReceived event.
    /// </summary>
    public class ErrorReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get { return Exception?.Message; } }

        /// <summary>
        /// Gets the name of the function or process that produced the error.
        /// </summary>
        public string Function { get; private set; }

        /// <summary>
        /// Gets the Error code returned by the socket.
        /// </summary>
        public SocketError SocketError { get; private set; }

        /// <summary>
        /// Gets more details on the error (the message of the inner exception).
        /// </summary>
        public string Details { get { return Exception?.InnerException?.Message; } }

        /// <summary>
        /// Gets the Exception that caused the error.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ErrorReceivedEventArgs"/>.
        /// </summary>
        /// <param name="ex">The exception that provides the error details.</param>
        /// <param name="memberName">The function that threw the error.</param>
        /// <returns>A new <see cref="ErrorReceivedEventArgs"/> instance.</returns>
        internal static ErrorReceivedEventArgs Create(Exception ex, string memberName = "")
        {
            ErrorReceivedEventArgs args = new ErrorReceivedEventArgs()
            {
                Function = memberName,
                Exception = ex,
                SocketError = SocketError.Success
            };
            if (ex is SocketException)
                args.SocketError = (ex as SocketException).SocketErrorCode;
            return args;
        }
    }
}
