using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Treorisoft.Net.Utilities;

namespace Treorisoft.Net
{
    /// <summary>
    /// Contains packet information for sending data while stored in the send queue.
    /// </summary>
    internal class SendPacket
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SendPacket"/> class with the specified destination and data.
        /// </summary>
        /// <param name="destination">The remote destination that the data should be sent to.</param>
        /// <param name="data">The data to send.</param>
        /// <remarks>Destination is only really used when sending via UDP.</remarks>
        public SendPacket(IPEndPoint destination, byte[] data)
        {
            ErrorCode = SocketError.Success;
            Destination = destination;
            Data = data;
            TotalLength = (data == null) ? 0 : data.Length;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SendPacket"/> class with the specified destination and data.
        /// </summary>
        /// <param name="destination">The remote destination that the data should be sent to.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="fileData">The <see cref="FileData"/> the data came from.</param>
        /// <param name="bufferSize">The size of the network buffer.</param>
        /// <param name="legacySupport">Whether the <see cref="Winsock"/> is in legacy mode or not.</param>
        /// <remarks>
        /// Destination is only really used when sending via UDP.
        /// The <see cref="FileData"/> is used to help determine which file is in the middle of being sent, and the progress of it.
        /// </remarks>
        public SendPacket(IPEndPoint destination, byte[] data, FileData fileData, int bufferSize, bool legacySupport) : this(destination, data)
        {
            FileData = fileData;
            if (fileData != null)
                TotalLength = legacySupport ? fileData.FileSize : fileData.GetTotalPartSize(bufferSize);
        }

        public byte[] GetDataToSend(int bufferSize, bool legacySupport)
        {
            if (_data != null && _data.Length > 0)
                return ArrayMethods.Shrink(ref _data, bufferSize);
            if (!IsFileData) return null;
            
            if (!FileDataHeaderSent && !legacySupport)
            {
                _data = ObjectPacker.Pack(FileData);
                PacketHeader.AddHeader(ref _data);
                FileDataHeaderSent = true;
                return ArrayMethods.Shrink(ref _data, bufferSize);
            }

            var part = FileData.GetNextPart(bufferSize);
            if(part != null)
            {
                if (!legacySupport)
                {
                    _data = ObjectPacker.Pack(part);
                    PacketHeader.AddHeader(ref _data);
                }
                else
                    _data = part.Data;
                return ArrayMethods.Shrink(ref _data, bufferSize);
            }
            return null;
        }

        /// <summary>
        /// Get the destination of the data should be sent to.
        /// </summary>
        public IPEndPoint Destination { get; private set; }

        private byte[] _data;
        /// <summary>
        /// Gets or sets the data that should be sent.
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private bool FileDataHeaderSent = false;

        /// <summary>
        /// Gets or sets the <see cref="FileData"/> associated with the data to be sent.
        /// </summary>
        public FileData FileData { get; private set; }

        /// <summary>
        /// Gets a boolean value indicating whether the data is associated with a <see cref="FileData"/> object.
        /// </summary>
        public bool IsFileData { get { return (FileData != null); } }

        /// <summary>
        /// Gets the <see cref="SocketError"/> that may have occurred during sending.
        /// </summary>
        public SocketError ErrorCode { get; internal set; }

        /// <summary>
        /// Gets the total length, in bytes, of the data being sent.
        /// </summary>
        public long TotalLength { get; private set; }

        /// <summary>
        /// Gets the total amount of data sent, in bytes.
        /// </summary>
        public long TotalSent { get; internal set; }

        /// <summary>
        /// Gets whether the data has been fully sent or not.
        /// </summary>
        internal bool SendCompleted { get { return (TotalSent == TotalLength); } }
    }
}
