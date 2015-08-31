using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Prepends a single value.
        /// </summary>
        /// <typeparam name="T">The type of the value to prepend.</typeparam>
        /// <param name="data">The sequence of elements.</param>
        /// <param name="value">The value to prepend.</param>
        /// <returns>The modified elements.</returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> data, T value)
        {
            return data.Prepend(new T[] { value });
        }
        /// <summary>
        /// Prepends the members of an <see cref="IEnumerable{T}"/> impelementation.
        /// </summary>
        /// <typeparam name="T">The type of the members of values.</typeparam>
        /// <param name="data">The sequence of elements.</param>
        /// <param name="values">A collection object that implements the <see cref="IEnumerable{T}"/> interface.</param>
        /// <returns>The modified elements.</returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> data, IEnumerable<T> values)
        {
            return values.Concat(data);
        }

        /// <summary>
        /// Concatenates a value to the given sequence.
        /// </summary>
        /// <typeparam name="T">Specifies the type of elements in the sequence.</typeparam>
        /// <param name="data">The sequence of elements.</param>
        /// <param name="value">The value to append to the sequence.</param>
        /// <returns>The concatenated members of data and value.</returns>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> data, T value)
        {
            return data.Concat(new T[] { value });
        }
    }
}
