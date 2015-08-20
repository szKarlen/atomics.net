using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading.Atomics
{
    [DebuggerDisplay("{Value}")]
    public sealed class Atomic<T> : IAtomic<T> where T : struct, IEquatable<T>
    {
        private T _value;
        private static readonly PrimitiveAtomics PrimitiveIntrinsics = new PrimitiveAtomics();

        private volatile IAtomic<T> _storage;
        private volatile object _instanceLock;

        private volatile MemoryOrder _order; // making volatile to prohibit reordering in constructors

        /// <summary>
        /// Creates new instance of <see cref="Atomic{T}"/>
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.SeqCst"/> semantics which hurt performance</param>
        public Atomic(MemoryOrder order = MemoryOrder.SeqCst)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order));

            _storage = GetStorage(order);

            if (order == MemoryOrder.SeqCst && _storage == this)
                _instanceLock = new object();

            _order = order;
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

            if (order == MemoryOrder.SeqCst && _storage == this)
                _instanceLock = new object();

            _order = order;
            _storage.Value = value;
        }

        private IAtomic<T> GetStorage(MemoryOrder order)
        {
            var type = typeof (T);
            if (type == typeof (bool))
            {
                return (IAtomic<T>)(IAtomic<bool>)new AtomicBoolean();
            }
            if (type == typeof(int))
            {
                return (IAtomic<T>)(IAtomic<int>)new AtomicInteger(order);
            }
            if (type == typeof(long))
            {
                return (IAtomic<T>)(IAtomic<long>)new AtomicLong();
            }
            return this;
        }

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
                var reader = PrimitiveIntrinsics as IAtomicsOperator<T> ?? this;

                if (_order != MemoryOrder.SeqCst) return reader.Read(ref _value);

                lock (_instanceLock)
                {
                    return reader.Read(ref _value);
                }
            }
            set
            {
                var writer = PrimitiveIntrinsics as IAtomicsOperator<T> ?? this;

                if (_order == MemoryOrder.SeqCst)
                {
                    lock (_instanceLock)
                    {
                        _value = value;
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
            if (!value.Equals(comparand))
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
            T result = location1;
            Interlocked.MemoryBarrier();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(Atomic<T> atomic)
        {
            return atomic.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Atomic<T>(T value)
        {
            return new Atomic<T>(value);
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