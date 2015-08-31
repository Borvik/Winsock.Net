using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net.Utilities
{
    /// <summary>
    /// Provides methods to act on an array.
    /// </summary>
    internal static class ArrayMethods
    {

        /// <summary>
        /// Removes items from the beginning of an array, and returns them.
        /// </summary>
        /// <typeparam name="T">The data type of the array to be shrunk.</typeparam>
        /// <param name="array">The one-dimensional, zero-based array that should be shrunk.</param>
        /// <param name="shrinkBy">A 32-bit integer that represents the number of items to remove from the array.</param>
        /// <returns>An array containin the items that were removed.</returns>
        public static T[] Shrink<T>(ref T[] array, int shrinkBy)
        {
            int newSize = array.Length - shrinkBy;
            if (newSize <= 0)
            {
                T[] remItems = array.Clone() as T[];
                array = null;
                return remItems;
            }

            T[] removedItems = new T[shrinkBy];
            Array.Copy(array, 0, removedItems, 0, shrinkBy);

            T[] destArray = new T[newSize];
            Array.Copy(array, shrinkBy, destArray, 0, newSize);

            array = destArray;
            return removedItems;
        }
    }
}
