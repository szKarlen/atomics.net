using System.Collections.Generic;
using System.Diagnostics;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="long"/> array wrapper with atomic operations
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class AtomicLongArray : IAtomicRefArray<long>, IReadOnlyCollection<long>
    {
        private readonly long[] _data;
        private readonly MemoryOrder _order;

        /// <summary>
        /// Creates new instance of <see cref="AtomicLongArray"/>
        /// </summary>
        /// <param name="source">The array to copy elements from</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicLongArray(long[] source, MemoryOrder order = MemoryOrder.SeqCst)
            : this(source.Length)
        {
            source.CopyTo(_data, 0);
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicLongArray"/>
        /// </summary>
        /// <param name="length">The length of the underlying array</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicLongArray(long length, MemoryOrder order = MemoryOrder.SeqCst)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order.ToString()));

            _data = new long[length];
            _order = order;
        }

        /// <summary>
        /// Sets an element at <paramref name="index"/> with provided <paramref name="order"/>
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> writes the value as <see cref="MemoryOrder.Acquire"/></remarks>
        public void Store(int index, ref long value, MemoryOrder order)
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
                    this._data[index] = value;
#else
                    Volatile.Write(ref this._data[index], value);
#endif
                    break;
                case MemoryOrder.SeqCst:
#if ARM_CPU || ITANIUM_CPU
                    Platform.MemoryBarrier();
                    this._data[index] = value;
                    Platform.MemoryBarrier();
#else
                    Interlocked.Exchange(ref this._data[index], value);
#endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets or sets the element at specified <paramref name="index"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public long this[int index]
        {
            get { return Load(index, _order); }
            set { Store(index, ref value, _order); }
        }

        /// <summary>
        /// Sets an element at <paramref name="index"/> with provided <paramref name="order"/>
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> writes the value as <see cref="MemoryOrder.Acquire"/></remarks>
        public void Store(int index, long value, MemoryOrder order)
        {
            Store(index, ref value, order);
        }

        /// <summary>
        /// Gets an element at <paramref name="index"/> with provided <paramref name="order"/>
        /// </summary>
        /// <param name="index">The index of element from which to load</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        public long Load(int index, MemoryOrder order)
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
#if ARM_CPU
                    var tmp = _storage.Slot.AcqRelValue;
                    Platform.MemoryBarrier();
                    return tmp;
#else
                    return Volatile.Read(ref this._data[index]);
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
        /// Atomically compares underlying element at specified <paramref name="index"/> with <paramref name="comparand"/> for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="index">The index of element to compare and set</param>
        /// <param name="value">The value that replaces the underlying value if the comparison results in equality</param>
        /// <param name="comparand">The value that is compared to the underlying value.</param>
        /// <returns>The original underlying value</returns>
        public long CompareExchange(int index, long value, long comparand)
        {
            return Interlocked.CompareExchange(ref this._data[index], value, comparand);
        }

        public int Count
        {
            get { return _data.Length; }
        }

        public IEnumerator<long> GetEnumerator()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                yield return Load(i, _order);
            }
        }

        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}