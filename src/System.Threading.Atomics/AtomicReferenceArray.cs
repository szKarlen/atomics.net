using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    public class AtomicReferenceArray<T> where T : class
    {
        private readonly volatile T[] _data;
        private readonly MemoryOrder _order;

        private readonly object _instanceLock;

        /// <summary>
        /// Creates new instance of <see cref="AtomicIntegerArray"/>
        /// </summary>
        /// <param name="source">The array to copy elements from</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicReferenceArray(T[] source, MemoryOrder order = MemoryOrder.SeqCst)
            : this(source.Length)
        {
            source.CopyTo(_data, 0);
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicIntegerArray"/>
        /// </summary>
        /// <param name="length">The length of the underlying array</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicReferenceArray(int length, MemoryOrder order = MemoryOrder.SeqCst)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order.ToString()));

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
        /// Sets atomically current <see cref="Value"/> by provided setter method
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
        /// Sets atomically current <see cref="Value"/> by provided setter method
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
        /// Sets atomically current <see cref="Value"/> by provided setter method
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
    }
}
