using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net.Utilities
{
    /// <summary>
    /// Provides data that allows a portion of a file to be sent over the network.
    /// </summary>
    [Serializable]
    internal class FileDataPart : ISerializable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FileDataPart"/>.
        /// </summary>
        /// <param name="fileGuid">A <see cref="Guid"/> identifying the file being sent.</param>
        /// <param name="start">The start position to start reading from the file.</param>
        /// <param name="data">The actual data to be sent over the network.</param>
        internal FileDataPart(Guid fileGuid, long start, byte[] data)
        {
            FileGuid = fileGuid;
            Start = start;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDataPart"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected FileDataPart(SerializationInfo info, StreamingContext context)
        {
            FileGuid = (Guid)info.GetValue("guid", typeof(Guid));
            Start = info.GetInt64("start");
            Data = (byte[])info.GetValue("data", typeof(byte[]));
        }

        /// <summary>
        /// Implements the <see cref="ISerializable"/> interface and returns the data needed to serialize the <see cref="FileDataPart"/> object.
        /// </summary>
        /// <param name="info">A SerializationInfo object containing information required to serialize the FileDataPart object.</param>
        /// <param name="context">A StreamingContext object containing the source and destination of the serialized stream associated with the FileDataPart object.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("guid", FileGuid);
            info.AddValue("start", Start);
            info.AddValue("data", Data);
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> assigned to the file to help identification.
        /// </summary>
        public Guid FileGuid { get; private set; }

        /// <summary>
        /// Gets the start position in the file for the data.
        /// </summary>
        public long Start { get; private set; }

        /// <summary>
        /// Gets the data sent/received over the network.
        /// </summary>
        public byte[] Data { get; private set; }
    }
}
