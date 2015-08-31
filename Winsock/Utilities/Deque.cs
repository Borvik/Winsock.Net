using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net.Utilities
{
    /// <summary>
    /// Represents both a first-in, first-out and a first-in, last-out collection of objects.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the deque</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    internal class Deque<T> : ICollection<T>, ICollection, IEnumerable<T>, ICloneable
    {
        /// <summary>
        /// The internal list to the <see cref="Deque{T}"/>.
        /// </summary>
        private LinkedList<T> list;

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class that is empty.
        /// </summary>
        public Deque() { list = new LinkedList<T>(); }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class that contains the elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="Deque{T}"/>.</param>
        public Deque(IEnumerable<T> collection) { list = new LinkedList<T>(collection); }

        #region ICollection<T> and IEnumerable<T> Members

        /// <summary>
        /// Gets the number of elements contained in the <see cref="Deque{T}"/>.
        /// </summary>
        public int Count { get { return list.Count; } }

        /// <summary>
        /// Gets a value that indicates whether access to this collection is read-only.
        /// </summary>
        /// <remarks>false in all cases.</remarks>
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="Deque{T}"/>.
        /// </summary>
        public object SyncRoot { get { return ((ICollection)list).SyncRoot; } }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="Deque{T}"/> is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized { get { return ((ICollection)list).IsSynchronized; } }

        [Browsable(false)]
        public void Add(T item) { throw new NotSupportedException("Use PushFront/PushBack."); }

        /// <summary>
        /// Removes all objects from the <see cref="Deque{T}"/>.
        /// </summary>
        public void Clear() { list.Clear(); }

        /// <summary>
        /// Determines whether an element is in the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="Deque{T}"/>. The value can be null for reference types.</param>
        /// <returns>true if item is found in the <see cref="Deque{T}"/>; otherwise false.</returns>
        public bool Contains(T item) { return list.Contains(item); }

        /// <summary>
        /// Copies the <see cref="Deque{T}"/> elements to an existing one-dimensional <see cref="Array"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from the <see cref="Deque{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array.Rank > 1) throw new ArgumentException("Array must have only a single dimension.", "array");
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the <see cref="Deque{T}"/> elements to an existing one-dimensional <see cref="Array"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from the <see cref="Deque{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index) { ((ICollection)list).CopyTo(array, index); }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>An <see cref="LinkedList{T}.Enumerator"/> for the <see cref="Deque{T}"/>.</returns>
        public IEnumerator<T> GetEnumerator() { return list.GetEnumerator(); }

        /// <summary>
        /// Removes the first occurrence of the specified value from the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The value to remove from the <see cref="Deque{T}"/>.</param>
        /// <returns>true if the element containing the value is successfully removed; otherwise, false. This method also returns false if value was not found in the original <see cref="Deque{T}"/>.</returns>
        public bool Remove(T item) { return list.Remove(item); }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>An <see cref="LinkedList{T}.Enumerator"/> for the <see cref="Deque{T}"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return list.GetEnumerator(); }

        #endregion

        #region ICloneable Members

        public object Clone() { return new Deque<T>(list); }

        #endregion

        /// <summary>
        /// Creates an array from the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>An array that contains the elements from the <see cref="Deque{T}"/>.</returns>
        public T[] ToArray() { return list.ToArray(); }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() { return list.ToString(); }

        /// <summary>
        /// Inserts an object at the back of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The object to push onto the <see cref="Deque{T}"/>. The value can be null for reference types.</param>
        public void PushBack(T item) { list.AddLast(item); }

        /// <summary>
        /// Inserts an object at the front of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The object to push onto the <see cref="Deque{T}"/>. The value can be null for reference types.</param>
        public void PushFront(T item) { list.AddFirst(item); }

        /// <summary>
        /// Returns the object at the back of the <see cref="Deque{T}"/> without removing it.
        /// </summary>
        /// <returns>The object at the back of the <see cref="Deque{T}"/>.</returns>
        public T PeekBack()
        {
            if (list.Count < 1) throw new InvalidOperationException("The deque is empty.");
            return list.Last.Value;
        }

        /// <summary>
        /// Returns the object at the front of the <see cref="Deque{T}"/> without removing it.
        /// </summary>
        /// <returns>The object at the front of the <see cref="Deque{T}"/>.</returns>
        public T PeekFront()
        {
            if (list.Count < 1) throw new InvalidOperationException("The deque is empty.");
            return list.First.Value;
        }

        /// <summary>
        /// Removes and returns the object at the back of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>The object removed from the back of the <see cref="Deque{T}"/>.</returns>
        public T PopBack()
        {
            if (list.Count < 1) throw new InvalidOperationException("The deque is empty.");
            var item = list.Last.Value;
            list.RemoveLast();
            return item;
        }

        /// <summary>
        /// Removes and returns the object at the front of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>The object removed from the front of the <see cref="Deque{T}"/>.</returns>
        public T PopFront()
        {
            if (list.Count < 1) throw new InvalidOperationException("The deque is empty.");
            var item = list.First.Value;
            list.RemoveFirst();
            return item;
        }
    }
}
