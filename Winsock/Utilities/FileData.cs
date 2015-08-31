using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Treorisoft.Net.Utilities
{
    /// <summary>
    /// Provides properties and instance methods for sending files over a <see cref="Winsock"/> objects.
    /// </summary>
    [Serializable]
    public class FileData : ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileData"/> class.
        /// </summary>
        /// <param name="info">A <see cref="FileInfo"/> object that contains data about the file.</param>
        public FileData(FileInfo info)
        {
            Guid = Guid.NewGuid();
            FileName = info.Name;
            FileSize = info.Length;
            Info = info;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileData"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected FileData(SerializationInfo info, StreamingContext context)
        {
            Guid = (Guid)info.GetValue("guid", typeof(Guid));
            FileName = info.GetString("filename");
            FileSize = info.GetInt64("filesize");
        }

        /// <summary>
        /// Implements the <see cref="ISerializable"/> interface and returns the data needed to serialize the <see cref="FileData"/> object.
        /// </summary>
        /// <param name="info">A SerializationInfo object containing information required to serialize the FileData object.</param>
        /// <param name="context">A StreamingContext object containing the source and destination of the serialized stream associated with the FileData object.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("guid", Guid);
            info.AddValue("filename", FileName);
            info.AddValue("filesize", FileSize);
        }

        /// <summary>
        /// Gets a <see cref="Guid"/> identifier for the file.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the size, in bytes, of the file.
        /// </summary>
        public long FileSize { get; private set; }

        /// <summary>
        /// Gets the <see cref="FileInfo"/> of the file being transmitted.
        /// </summary>
        /// <remarks>
        /// During sending this contains the details of the actual file being transmitted.
        /// During receiving this contains the details of a temporary file, that upon arrival can be moved to another location.
        /// </remarks>
        public FileInfo Info { get; internal set; }

        #region Sending/Receiving Helpers

        /// <summary>
        /// Field backing the SyncRoot property.
        /// </summary>
        internal object _syncRoot = null;

        /// <summary>
        /// Field backing the MetaSize property.
        /// </summary>
        internal static int _fileDataMetaSize = -1;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the file being read/written.
        /// </summary>
        private object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        /// <summary>
        /// Gets a value the estimates the size of an empty <see cref="FileDataPart"/> object.
        /// </summary>
        internal static int MetaSize
        {
            get
            {
                if (_fileDataMetaSize < 0)
                    _fileDataMetaSize = EstimatePartSize(0) + 1;
                return _fileDataMetaSize;
            }
        }

        /// <summary>
        /// Gets an estimate of a <see cref="FileDataPart"/> object of the specified data size.
        /// </summary>
        /// <param name="size">The size of the data that should be used in the size estimate.</param>
        /// <returns>The estimated size, in bytes, of a serialized <see cref="FileDataPart"/>.</returns>
        internal static int EstimatePartSize(int size)
        {
            return EstimatePartSize(Guid.Empty, long.MaxValue, size);
        }

        /// <summary>
        /// Gets an estimate of a <see cref="FileDataPart"/>.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> that should be used in the size estimate.</param>
        /// <param name="start">The start position that should be used in the size estimate.</param>
        /// <param name="size">The size of the data that should be used in the size estimate.</param>
        /// <returns>The estimated size, in bytes, of a serialized <see cref="FileDataPart"/>.</returns>
        internal static int EstimatePartSize(Guid guid, long start, int size)
        {
            var part = new FileDataPart(guid, start, new byte[size]);
            var bytes = ObjectPacker.Pack(part);
            return bytes.Length + PacketHeader.HeaderSize(bytes.Length);
        }

        #endregion

        #region Sending

        /// <summary>
        /// Gets the total number of bytes sent.
        /// </summary>
        internal long SentBytes { get; set; } = 0;

        /// <summary>
        /// Gets a boolean value indicating whether if the file has been completely sent or not.
        /// </summary>
        internal bool SendCompleted { get { return (SentBytes == FileSize); } }

        /// <summary>
        /// Gets the next <see cref="FileDataPart"/> that will be sent over the network.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer and amount of data to return.</param>
        /// <returns>The bufferSize is adjusted by an estimate of how large the metadata would be, so that the size of the serialized <see cref="FileDataPart"/> should be the same as the bufferSize.</returns>
        internal FileDataPart GetNextPart(int bufferSize)
        {
            if (SendCompleted) return null;
            if (Info == null) throw new NullReferenceException("File information not found.");
            if (!Info.Exists) throw new FileNotFoundException(Info.FullName);

            bufferSize -= MetaSize;
            byte[] buffer = new byte[bufferSize];
            lock (SyncRoot)
            {
                using (FileStream fs = new FileStream(Info.FullName, FileMode.Open, FileAccess.Read))
                {
                    fs.Seek(SentBytes, SeekOrigin.Begin);
                    int readSize = fs.Read(buffer, 0, bufferSize);
                    byte[] sizedBuffer = ArrayMethods.Shrink(ref buffer, readSize);
                    var part = new FileDataPart(Guid, SentBytes, sizedBuffer);
                    SentBytes += readSize;
                    return part;
                }
            }
        }

        /// <summary>
        /// Gets the total size of all the parts of the file at the given buffer size.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer and amount of data to estimate with.</param>
        /// <returns>
        /// This routine is used by <see cref="SendProgress"/> to help determine the total number of bytes to send, and therefore also the percent completed.
        /// The bufferSize is adjusted by an estimate of how large the metadata would be, so that the size of the serialized <see cref="FileDataPart"/> should be the same as the bufferSize.
        /// </returns>
        internal long GetTotalPartSize(int bufferSize)
        {
            long sum = 0;
            foreach (int size in GetPartSizes(bufferSize))
                sum += size;
            return sum;
        }

        /// <summary>
        /// Gets the sizes of all the parts of the file at the given buffer size.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer and amount of data to estimate with.</param>
        /// <returns>
        /// Returns an IEnumerable containing the sizes of all the different parts of the file - including the <see cref="FileData"/> header.
        /// </returns>
        internal IEnumerable<int> GetPartSizes(int bufferSize)
        {
            if (Info == null) throw new NullReferenceException("File information not found.");
            if (!Info.Exists) throw new FileNotFoundException(Info.FullName);

            var bytes = ObjectPacker.Pack(this);
            yield return bytes.Length + PacketHeader.HeaderSize(bytes.Length);

            bufferSize -= MetaSize;
            long fileSize = Info.Length;
            long start = 0;
            while (fileSize > 0)
            {
                if (fileSize > bufferSize)
                {
                    yield return EstimatePartSize(Guid, start, bufferSize);
                    fileSize -= bufferSize;
                    start += bufferSize;
                }
                else
                {
                    yield return EstimatePartSize(Guid, start, (int)fileSize);
                    fileSize = 0;
                }
            }
        }
        #endregion

        #region Receiving

        /// <summary>
        /// Gets the total number of received bytes.
        /// </summary>
        public long ReceivedBytes { get; private set; } = 0;

        /// <summary>
        /// Gets a boolean value indicating if the file has been completely received or not.
        /// </summary>
        public bool ReceiveCompleted { get { return (ReceivedBytes == FileSize); } }

        /// <summary>
        /// Gets the size of the last received <see cref="FileDataPart"/>.
        /// </summary>
        /// <remarks>This is used to help setup the <see cref="ReceiveProgressEventArgs"/> that is raised.</remarks>
        internal int LastReceivedSize { get; set; }

        /// <summary>
        /// Receives the <see cref="FileDataPart"/> into the specified file.
        /// </summary>
        /// <param name="fileName">The fully qualified name of the file, or the relative file name. Do not end the path with the directory separator character.</param>
        /// <param name="part">The <see cref="FileDataPart"/> to be received.</param>
        internal void ReceivePart(string fileName, FileDataPart part)
        {

            ReceivePart(new FileInfo(fileName), part);
        }

        /// <summary>
        /// Receives the <see cref="FileDataPart"/> into the specified file.
        /// </summary>
        /// <param name="file">A <see cref="FileInfo"/> object containing the fully qualified name of the file to receiving the data to.</param>
        /// <param name="part">The <see cref="FileDataPart"/> to be received.</param>
        internal void ReceivePart(FileInfo file, FileDataPart part)
        {
            if (ReceiveCompleted)
                throw new InvalidOperationException("Receive operation for this file has already been completed.");

            lock (SyncRoot)
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Seek(part.Start, SeekOrigin.Begin);
                    fs.Write(part.Data, 0, part.Data.Length);
                }
                LastReceivedSize = part.Data.Length;
                ReceivedBytes += part.Data.Length;
            }
        }

        #endregion

    }
}
