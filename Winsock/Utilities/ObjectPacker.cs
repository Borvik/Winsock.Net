using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net.Utilities
{
    /// <summary>
    /// Prepares an object, or an entire graph of connected objects for transmission.
    /// </summary>
    internal class ObjectPacker
    {
        /// <summary>
        /// Prepares an object, or an entire graph of connected objects for sending across the network.
        /// </summary>
        /// <param name="@object">The object to serialize for sending.</param>
        /// <returns>The serialized data in a <see cref="byte"/> array.</returns>
        internal static byte[] Pack(object @object)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                try
                {
                    bf.Serialize(ms, @object);
                    return ms.ToArray();
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// Gets an object, or an entire graph of connected objects from a serialized <see cref="byte"/> array.
        /// </summary>
        /// <param name="box">The <see cref="byte"/> array containing the object.</param>
        /// <returns>The deserialized object, or on failure the original <see cref="byte"/> array.</returns>
        internal static object Unpack(byte[] box)
        {
            using (var ms = new MemoryStream(box, false))
            {
                var bf = new BinaryFormatter();
                try
                {
                    return bf.Deserialize(ms);
                }
                catch { return box; }
            }
        }
    }
}
