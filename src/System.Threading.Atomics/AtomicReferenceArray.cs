using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="T"/> array wrapper with atomic operations
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class AtomicReferenceArray<T> : ICollection<T>,
        IReadOnlyCollection<T>,
        IStructuralComparable,
        IStructuralEquatable where T : class
    {
        private readonly T[] _data;
        private readonly MemoryOrder _order;

        private readonly object _instanceLock;

        /// <summary>
        /// Creates new instance of <see cref="AtomicIntegerArray"/>
        /// </summary>
        /// <param name="source">The array to copy elements from</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicReferenceArray(T[] source, MemoryOrder order = MemoryOrder.SeqCst)
        {
            if (source == null) throw new ArgumentNullException("source");
            order.ThrowIfNotSupported();

            _data = new T[source.Length];
            _order = order;

            source.CopyTo(_data, 0);

            _instanceLock = order == MemoryOrder.SeqCst ? new object() : null;
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicIntegerArray"/>
        /// </summary>
        /// <param name="length">The length of the underlying array</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicReferenceArray(int length, MemoryOrder order = MemoryOrder.SeqCst)
        {
            if (length < 0) throw new ArgumentException("Length can't be negative");
            order.ThrowIfNotSupported();

            _data = new T[length];
            _order = order;

            _instanceLock = order == MemoryOrder.SeqCst ? new object() : null;
        }

        /// <summary>
        /// Gets or sets the element at specified <paramref name="index"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return _order != MemoryOrder.SeqCst ? _data[index] : Volatile.Read(ref _data[index]); }
            set
            {
                if (_order == MemoryOrder.SeqCst)
                {
                    Interlocked.Exchange(ref _data[index], value); // for processors cache coherence
                    return;
                }

                T currentValue;
                T tempValue;
                do
                {
                    currentValue = _data[index];
                    tempValue = value;
                } while (_data[index] != currentValue || Interlocked.CompareExchange(ref _data[index], tempValue, currentValue) != currentValue);
            }
        }

        /// <summary>
        /// Sets an element at <paramref name="index"/> with provided <paramref name="order"/>
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> writes the value as <see cref="MemoryOrder.Acquire"/></remarks>
        public void Store(int index, T value, MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    this._data[index] = value;
                    break;
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    throw new InvalidOperationException("Cannot set (store) value with Acquire semantics");
                case MemoryOrder.Release:
                case MemoryOrder.AcqRel:
#if ARM_CPU || ITANIUM_CPU
                    Platform.MemoryBarrier();
#endif
                    this._data[index] = value;
                    break;
                case MemoryOrder.SeqCst:
                    Interlocked.Exchange(ref _data[index], value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets an element at <paramref name="index"/> with provided <paramref name="order"/>
        /// </summary>
        /// <param name="index">The index of element from which to load</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        public T Load(int index, MemoryOrder order)
        {
            if (order == MemoryOrder.Consume)
                throw new NotSupportedException();
            if (order == MemoryOrder.Release)
                throw new InvalidOperationException("Cannot get (load) value with Release semantics");
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    return this._data[index];
                case MemoryOrder.Acquire:
                case MemoryOrder.AcqRel:
                case MemoryOrder.SeqCst:
#if ARM_CPU || ITANIUM_CPU
                    var tmp = this._data[index];
                    Platform.MemoryBarrier();
                    return tmp;
#else
                    return this._data[index];
#endif
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        public bool IsLockFree
        {
            get { return true; }
        }

        /// <summary>
        /// Atomically sets the item at zero-based index (<paramref name="index"/>) to the given value by provided setter method and return the old value
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="setter">The setter to use</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>An updated value</returns>
        public T Set(int index, Func<T, T> setter, MemoryOrder order)
        {
            bool lockTaken = false;
            if (order == MemoryOrder.SeqCst)
                Monitor.Enter(_instanceLock, ref lockTaken);
            try
            {
                T currentValue;
                T tempValue;
                do
                {
                    currentValue = _data[index];
                    tempValue = setter(currentValue);
                } while (_data[index] != currentValue || Interlocked.CompareExchange(ref _data[index], tempValue, currentValue) != currentValue);
                return currentValue;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_instanceLock);
            }
        }

        /// <summary>
        /// Atomically sets the item at zero-based index (<paramref name="index"/>) to the given value by provided setter method and return the old value
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="setter">The setter to use</param>
        /// <returns>An updated value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Set(int index, Func<T, T> setter)
        {
            return Set(index, setter, this._order);
        }

        /// <summary>
        /// Sets atomically current <see cref="Value"/> by provided setter method
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="setter">The setter to use</param>
        /// <param name="data">Any arbitrary value to be passed to <paramref name="setter"/></param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>An updated value</returns>
        public T Set<TData>(int index, Func<T, TData, T> setter, TData data, MemoryOrder order)
        {
            bool lockTaken = false;
            if (order == MemoryOrder.SeqCst)
                Monitor.Enter(_instanceLock, ref lockTaken);
            try
            {
                T currentValue;
                T tempValue;
                do
                {
                    currentValue = _data[index];
                    tempValue = setter(currentValue, data);
                } while (_data[index] != currentValue || Interlocked.CompareExchange(ref _data[index], tempValue, currentValue) != currentValue);
                return currentValue;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_instanceLock);
            }
        }

        /// <summary>
        /// Atomically sets the item at zero-based index (<paramref name="index"/>) to the given value by provided setter method and return the old value
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="setter">The setter to use</param>
        /// <param name="data">Any arbitrary value to be passed to <paramref name="setter"/></param>
        /// <returns>An updated value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Set<TData>(int index, Func<T, TData, T> setter, TData data)
        {
            return Set(index, setter, data, this._order);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="AtomicReference{T}"/>.
        /// </summary>
        /// <value>The number of elements contained in the <see cref="AtomicReference{T}"/>.</value>
        public int Count
        {
            get { return _data.Length; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                yield return Load(i, _order);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException("Collection is of a fixed size");
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException("Collection is of a fixed size");
        }

        /// <summary>
        /// Determines whether the <see cref="AtomicReference{T}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="AtomicReference{T}"/>.</param>
        /// <returns>true if item is found in the <see cref="AtomicReference{T}"/>; otherwise, false.</returns>
        public bool Contains(T item)
        {
            foreach (var i in _data)
            {
                if (i == item)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Copies the elements of the <see cref="AtomicReference{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="AtomicReference{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException("Collection is of a fixed size");
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            return ((IStructuralComparable)_data).CompareTo(other, comparer);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)_data).Equals(other, comparer);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)_data).GetHashCode(comparer);
        }
    }
}
