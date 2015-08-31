using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net.Utilities
{
    /// <summary>
    /// Manages a set of <see cref="byte"/> values received via the network.
    /// </summary>
    internal class ByteBufferCollection : CollectionBase
    {
        /// <summary>
        /// Adds an item to the <see cref="ByteBufferCollection"/>.
        /// </summary>
        /// <param name="value">The value to add to the <see cref="ByteBufferCollection"/>.</param>
        public void Add(byte value)
        {
            List.Add(value);
        }

        /// <summary>
        /// Adds an array of <see cref="byte"/> values to the current <see cref="ByteBufferCollection"/>.
        /// </summary>
        /// <param name="values">The values to add to the <see cref="ByteBufferCollection"/>.</param>
        public void Add(byte[] values)
        {
            for (int i = 0; i < values.Length; i++)
                List.Add(values[i]);
        }

        /// <summary>
        /// Creates an <see cref="byte"/> array from the current <see cref="ByteBufferCollection"/>.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] ToArray()
        {
            if (List.Count == 0) return null;
            return List.Cast<byte>().ToArray();
        }

        /// <summary>
        /// Concatenates this sequence of bytes with the passed list of bytes.
        /// </summary>
        /// <param name="data">The list of bytes to append (but not store) to the current <see cref="ByteBufferCollection"/>.</param>
        /// <returns>The concatenated sequence of bytes.</returns>
        public byte[] Combine(byte[] data)
        {
            return List.Cast<byte>().Concat(data).ToArray();
        }

        /// <summary>
        /// Pops specified number of elements off the front of the byte buffer.
        /// </summary>
        /// <param name="count">The number of elements to get.</param>
        /// <returns>An array of bytes.</returns>
        public byte[] Pop(int count)
        {
            if (List.Count < count)
                throw new ArgumentException("Count may not be below the size of the buffer.");

            byte[] data = List.Cast<byte>().Take(count).ToArray();
            byte[] rem = List.Cast<byte>().Skip(count).ToArray();
            List.Clear();
            Add(rem);
            return data;
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ByteBufferCollection"/>.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return ((ICollection)this).SyncRoot;
            }
        }
    }
}
