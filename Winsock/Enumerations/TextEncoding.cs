using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Represents a character encoding.
    /// </summary>
    public enum TextEncoding
    {
        /// <summary>
        /// Represents the encoding for the ASCII (7-bit) character set.
        /// </summary>
        ASCII,
        /// <summary>
        /// Represents the encoding for the UTF-16 format that uses the big endian byte order.
        /// </summary>
        BigEndianUnicode,
        /// <summary>
        /// Represents that the value in <see cref="Winsock.CustomTextEncoding"/> should be used as the text encoder.
        /// </summary>
        Custom,
        /// <summary>
        /// Represents the encoding for the operating system's current ANSI code page.
        /// </summary>
        Default,
        /// <summary>
        /// Represents the encoding for the UTF-16 format using the little endian byte order.
        /// </summary>
        Unicode,
        /// <summary>
        /// Represents the encoding for the UTF-7 format.
        /// </summary>
        UTF7,
        /// <summary>
        /// Represents the encoding for the UTF-8 format.
        /// </summary>
        UTF8,
        /// <summary>
        /// Represents the encoding for the UTF-32 format using the little endian byte order.
        /// </summary>
        UTF32
    }
}
