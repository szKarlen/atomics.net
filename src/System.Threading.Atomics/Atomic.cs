using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Threading.Atomics
{
    /// <summary>
    /// A wrapper for structs with atomic operations
    /// </summary>
    /// <typeparam name="T">The underlying struct's type</typeparam>
    [DebuggerDisplay("{Value}")]
#pragma warning disable 0659, 0661
    public sealed class Atomic<T> : IAtomic<T>, IEquatable<T>, IEquatable<Atomic<T>> where T : struct, IEquatable<T>
#pragma warning restore 0659, 0661
    {
        private T _value;
        private static readonly int PodSize = Marshal.SizeOf(typeof (T));
        private static readonly IAtomicsOperator<T> Intrinsics = new PrimitiveAtomics() as IAtomicsOperator<T>;

        private readonly IAtomic<T> _storage;
        private readonly object _instanceLock;

        private readonly MemoryOrder _order;
        private readonly IAtomicsOperator<T> _readerWriter;

        /// <summary>
        /// Creates new instance of <see cref="Atomic{T}"/>
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.SeqCst"/> semantics which hurt performance</param>
        public Atomic(MemoryOrder order = MemoryOrder.SeqCst)
            :this (default(T), order)
        {
            
        }

        /// <summary>
        /// Creates new instance of <see cref="Atomic{T}"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.SeqCst"/> semantics which hurt performance</param>
        public Atomic(T value, MemoryOrder order = MemoryOrder.SeqCst)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order));

            _storage = GetStorage(order);

            if (Intrinsics != null && Intrinsics.Supports<T>())
            {
                _readerWriter = Intrinsics;
            }
            else if (_storage as Atomic<T> == this)
            {
                _readerWriter = this;
            }
            else if (_storage.Supports<T>())
            {
                _readerWriter = _storage;
            }
            else
            {
                throw new NotSupportedException(string.Format("{0} type is not supported", typeof(T)));
            }

            if (order == MemoryOrder.SeqCst && object.ReferenceEquals(_storage, this))
                _instanceLock = new object();

            _order = order;
            _storage.Value = value;
        }

        private IAtomic<T> GetStorage(MemoryOrder order)
        {
            var type = typeof (T);
            if (type == typeof (bool))
            {
                return (IAtomic<T>)(IAtomic<bool>)new AtomicBoolean(order);
            }
            if (type == typeof(int))
            {
                return (IAtomic<T>)(IAtomic<int>)new AtomicInteger(order);
            }
            if (type == typeof(long))
            {
                return (IAtomic<T>)(IAtomic<long>)new AtomicLong(order);
            }
            return this;
        }

        /// <summary>
        /// Gets or sets atomically the underlying value
        /// </summary>
        /// <remarks>This method does use CAS approach for value setting. To avoid this use <see cref="Load"/> and <see cref="Store"/> methods pair for get/set operations respectively</remarks>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _storage.Value; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _storage.Value = value; }
        }

        T IAtomic<T>.Value
        {
            get
            {
                if (_order != MemoryOrder.SeqCst) return _readerWriter.Read(ref _value);

                lock (_instanceLock)
                {
                    return _readerWriter.Read(ref _value);
                }
            }
            set
            {
                var writer = _readerWriter;

                if (_order == MemoryOrder.SeqCst)
                {
                    lock (_instanceLock)
                    {
                        Platform.WriteSeqCst(ref _value, ref value);
                    }
                }
                else if (_order.IsAcquireRelease())
                {
                    T currentValue = this._value;
                    T tempValue;
                    do
                    {
                        tempValue = writer.CompareExchange(ref this._value, value, currentValue);
                    } while (!tempValue.Equals(currentValue));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T IAtomicsOperator<T>.CompareExchange(ref T location1, T value, T comparand)
        {
            T temp = location1;
            if (!temp.Equals(comparand))
            {
                Interlocked.MemoryBarrier();
                location1 = value;
            }

            Interlocked.MemoryBarrier();
            return temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T IAtomicsOperator<T>.Read(ref T location1)
        {
            return Platform.Read(ref location1);
        }

        bool IAtomicsOperator<T>.Supports<TType>()
        {
            return true;
        }

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        public bool IsLockFree
        {
            get { return _order != MemoryOrder.SeqCst || (_order.IsAcquireRelease() && PodSize <= IntPtr.Size); }
        }

        void IAtomic<T>.Store(T value, MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    if (ReferenceEquals(_storage, this))
                        Platform.Write(ref _value, ref value);
                    break;
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    throw new InvalidOperationException("Cannot set (store) value with Acquire semantics");
                case MemoryOrder.Release:
                case MemoryOrder.AcqRel:
                    Platform.WriteRelease(ref _value, ref value);
                    break;
                case MemoryOrder.SeqCst:
                    Platform.WriteSeqCst(ref _value, ref value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        public void Store(T value, MemoryOrder order)
        {
            _storage.Store(value, order);
        }

        T IAtomic<T>.Load(MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    return Platform.Read(ref _value);
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    return Platform.ReadAcquire(ref _value);
                case MemoryOrder.Release:
                    throw new InvalidOperationException("Cannot get (load) value with Release semantics");
                case MemoryOrder.AcqRel:
                    return Platform.ReadAcquire(ref _value);
                case MemoryOrder.SeqCst:
                    if (PodSize > IntPtr.Size)
                        lock (_instanceLock)
                            return Platform.ReadSeqCst(ref _value);

                    return Platform.ReadSeqCst(ref _value);
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        public T Load(MemoryOrder order)
        {
            return _storage.Load(order);
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="AtomicInteger"/> to a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="atomic">The <see cref="Atomic{T}"/> to convert.</param>
        /// <returns>The converted <see cref="Atomic{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(Atomic<T> atomic)
        {
            return atomic == null ? default(T) : atomic.Value;
        }

        /// <summary>
        /// Defines an implicit conversion of a <typeparamref name="T"/> to a <see cref="AtomicInteger"/>.
        /// </summary>
        /// <param name="value">The <typeparamref name="T"/> to convert.</param>
        /// <returns>The converted <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Atomic<T>(T value)
        {
            return new Atomic<T>(value);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="Atomic{T}"/> and equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            Atomic<T> other = obj as Atomic<T>;
            if (other == null) return false;

            return object.ReferenceEquals(this, other) || this.Value.Equals(other.Value);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <paramref name="other"/> represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(T other)
        {
            return this.Value.Equals(other);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="Atomic{T}"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Atomic<T> other)
        {
            return !object.ReferenceEquals(other, null) && (object.ReferenceEquals(this, other) || this.Value.Equals(other.Value));
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="Atomic{T}"/> and <typeparamref name="T"/> are equal.
        /// </summary>
        /// <param name="x">The first value (<see cref="Atomic{T}"/>) to compare.</param>
        /// <param name="y">The second value (<typeparamref name="T"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Atomic<T> x, T y)
        {
            return (!object.ReferenceEquals(x, null) && x.Value.Equals(y));
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="Atomic{T}"/> and <see cref="Atomic{T}"/> are equal.
        /// </summary>
        /// <param name="x">The first value (<see cref="AtomicInteger"/>) to compare.</param>
        /// <param name="y">The second value (<see cref="int"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Atomic<T> x, Atomic<T> y)
        {
            return (!object.ReferenceEquals(x, null) && x.Equals(y));
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="Atomic{T}"/> and <typeparamref name="T"/> have different values.
        /// </summary>
        /// <param name="x">The first value (<see cref="Atomic{T}"/>) to compare.</param>
        /// <param name="y">The second value (<typeparamref name="T"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Atomic<T> x, T y)
        {
            if (object.ReferenceEquals(x, null))
                return false;

            T value = x.Value;
            return !value.Equals(y);
        }

        public static bool operator !=(Atomic<T> x, Atomic<T> y)
        {
            if (object.ReferenceEquals(x, null))
                return false;

            return !x.Equals(y);
        }

        private class PrimitiveAtomics : IAtomicsOperator<int>,
            IAtomicsOperator<long>,
            IAtomicsOperator<double>,
            IAtomicsOperator<float>,
            IAtomicsOperator<uint>,
            IAtomicsOperator<ulong>,
            IAtomicsOperator<sbyte>,
            IAtomicsOperator<byte>,
            IAtomicsOperator<short>,
            IAtomicsOperator<ushort>
        {
            public bool Supports<TType>() where TType : struct
            {
                return this as IAtomicsOperator<TType> != null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int IAtomicsOperator<int>.CompareExchange(ref int location1, int value, int comparand)
            {
                return Interlocked.CompareExchange(ref location1, value, comparand);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int IAtomicsOperator<int>.Read(ref int location1)
            {
                return Volatile.Read(ref location1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            long IAtomicsOperator<long>.CompareExchange(ref long location1, long value, long comparand)
            {
                return Interlocked.CompareExchange(ref location1, value, comparand);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            long IAtomicsOperator<long>.Read(ref long location1)
            {
                return Interlocked.Read(ref location1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            double IAtomicsOperator<double>.CompareExchange(ref double location1, double value, double comparand)
            {
                return Interlocked.CompareExchange(ref location1, value, comparand);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            double IAtomicsOperator<double>.Read(ref double location1)
            {
                return Volatile.Read(ref location1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            float IAtomicsOperator<float>.CompareExchange(ref float location1, float value, float comparand)
            {
                return Interlocked.CompareExchange(ref location1, value, comparand);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            float IAtomicsOperator<float>.Read(ref float location1)
            {
                return Volatile.Read(ref location1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe uint IAtomicsOperator<uint>.CompareExchange(ref uint location1, uint value, uint comparand)
            {
                fixed (uint* ptr = &location1)
                {
                    var result = Interlocked.CompareExchange(ref *(int*)ptr, *(int*)&value, *(int*)&comparand);
                    return *(uint*)&result;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe uint IAtomicsOperator<uint>.Read(ref uint location1)
            {
                fixed (uint* ptr = &location1)
                {
                    var result = Volatile.Read(ref *(int*)ptr);
                    return *(uint*)&result;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe ulong IAtomicsOperator<ulong>.CompareExchange(ref ulong location1, ulong value, ulong comparand)
            {
                fixed (ulong* ptr = &location1)
                {
                    var result = Interlocked.CompareExchange(ref *(long*) ptr, *(long*)&value, *(long*)&comparand);
                    return *(ulong*)&result;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe ulong IAtomicsOperator<ulong>.Read(ref ulong location1)
            {
                fixed (ulong* ptr = &location1)
                {
                    var result = Volatile.Read(ref *(long*)ptr);
                    return *(ulong*)&result;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            sbyte IAtomicsOperator<sbyte>.CompareExchange(ref sbyte location1, sbyte value, sbyte comparand)
            {
                sbyte temp = location1;
                if (temp != comparand)
                    Volatile.Write(ref location1, value);

                Interlocked.MemoryBarrier();
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            sbyte IAtomicsOperator<sbyte>.Read(ref sbyte location1)
            {
                return Volatile.Read(ref location1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            byte IAtomicsOperator<byte>.CompareExchange(ref byte location1, byte value, byte comparand)
            {
                byte temp = location1;
                if (temp != comparand)
                    Volatile.Write(ref location1, value);

                Interlocked.MemoryBarrier();
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            byte IAtomicsOperator<byte>.Read(ref byte location1)
            {
                return Volatile.Read(ref location1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            short IAtomicsOperator<short>.CompareExchange(ref short location1, short value, short comparand)
            {
                short temp = location1;
                if (temp != comparand)
                    Volatile.Write(ref location1, value);

                Interlocked.MemoryBarrier();
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            short IAtomicsOperator<short>.Read(ref short location1)
            {
                return Volatile.Read(ref location1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            ushort IAtomicsOperator<ushort>.CompareExchange(ref ushort location1, ushort value, ushort comparand)
            {
                ushort temp = location1;
                if (temp != comparand)
                    Volatile.Write(ref location1, value);

                Interlocked.MemoryBarrier();
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            ushort IAtomicsOperator<ushort>.Read(ref ushort location1)
            {
                return Volatile.Read(ref location1);
            }
        }
    }
}