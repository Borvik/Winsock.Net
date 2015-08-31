using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net.Utilities
{
    /// <summary>
    /// Describes a serialized packet of information transmitted over the network.
    /// </summary>
    internal class PacketHeader
    {
        private int _size = -1;
        private byte? _delimiter = null;

        /// <summary>
        /// Gets the size, in bytes, of the transmitted packet.
        /// </summary>
        public int Size { get { return _size; } }

        /// <summary>
        /// Gets the delimiter that will be used to separate the Size information from the actual transmitted data.
        /// </summary>
        public byte Delimiter
        {
            get { return _delimiter.HasValue ? _delimiter.Value : byte.MinValue; }
            private set { _delimiter = value; }
        }

        private bool HasDelimiter { get { return _delimiter.HasValue; } }
        private bool HasSize { get { return Size > -1; } }

        /// <summary>
        /// Gets a value indicating that the incoming <see cref="PacketHeader"/> has been fully processed.
        /// </summary>
        public bool Completed { get { return HasDelimiter && HasSize; } }

        /// <summary>
        /// Resets the <see cref="PacketHeader"/> so it is ready for the next set of incoming data.
        /// </summary>
        public void Reset()
        {
            _delimiter = null;
            _size = -1;
        }

        /// <summary>
        /// Processes the incoming <see cref="byte"/> array to see if a <see cref="PacketHeader"/> is within it.
        /// </summary>
        /// <param name="data">The data to check for a <see cref="PacketHeader"/> - may be incomplete and the rest stored in the buffer.</param>
        /// <param name="buffer">A <see cref="ByteBufferCollection"/> that potentially has stored part of a <see cref="PacketHeader"/>.</param>
        /// <returns>true, if the method encountered no error; otherwise false.</returns>
        /// <remarks>Just because the method returns true, does not mean it found the <see cref="PacketHeader"/> - just that there were no errors.</remarks>
        public bool ProcessHeader(ref byte[] data, ref ByteBufferCollection buffer)
        {
            if (!HasDelimiter)
            {
                Delimiter = ArrayMethods.Shrink(ref data, 1)[0];
                if (data == null || data.Length == 0) return true;
            }

            int delimiterIndex = Array.IndexOf(data, Delimiter);
            if (delimiterIndex == -1)
            {
                buffer.Add(data);
                return true;
            }

            byte[] headerData = buffer.Combine(ArrayMethods.Shrink(ref data, delimiterIndex + 1));
            Array.Resize(ref headerData, headerData.Length - 1);
            buffer.Clear();

            string size = Encoding.ASCII.GetString(headerData);
            if (!int.TryParse(size, out _size))
            {
                Reset();
                return false;
            }

            if (Size == 0)
                Reset();
            return true;
        }

        /// <summary>
        /// Adds <see cref="PacketHeader"/> information to the given data.
        /// </summary>
        /// <param name="data">The data to calculate and add <see cref="PacketHeader"/> information to.</param>
        public static void AddHeader(ref byte[] data)
        {
            byte[] header = Encoding.ASCII.GetBytes(data.Length.ToString());
            byte delimiter = FindUnusedByte(header);
            data = header.Prepend(delimiter).Concat(delimiter).Concat(data).ToArray();
        }

        /// <summary>
        /// Calculates the size of <see cref="PacketHeader"/> information given the size of the data.
        /// </summary>
        /// <param name="length">The size of the data to calculate the header size for.</param>
        /// <returns>The size of the <see cref="PacketHeader"/>.</returns>
        public static int HeaderSize(int length)
        {
            byte[] header = Encoding.ASCII.GetBytes(length.ToString());
            return header.Length + 2;
        }


        private static byte FindUnusedByte(byte[] data)
        {
            var unique = data.Distinct();
            byte low = 0;
            for (byte i = 1; i <= byte.MaxValue; i++)
            {
                if (!unique.Contains(i))
                    return i;
            }
            return low;
        }

    }
}
