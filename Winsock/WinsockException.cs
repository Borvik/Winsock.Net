using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// The exception that is thrown when a <see cref="Winsock"/> error occurs.
    /// </summary>
    [Serializable]
    public class WinsockException : Exception
    {
        /// <summary>
        /// Intializes a new instance of the <see cref="WinsockException"/> class with the last operating system error code.
        /// </summary>
        public WinsockException() { }

        /// <summary>
        /// Intializes a new instance of the <see cref="WinsockException"/> class with the specified error message.
        /// </summary>
        public WinsockException(string message) : base(message) { }

        /// <summary>
        /// Intializes a new instance of the <see cref="WinsockException"/> class with the specified error message, and the specified exception.
        /// </summary>
        public WinsockException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Intializes a new instance of the <see cref="WinsockException"/> class from the specified instances of the <see cref="SerializationInfo"/> and <see cref="StreamingContext"/> classes.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> associated with this exception.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that represents the context of this exception.</param>
        protected WinsockException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
