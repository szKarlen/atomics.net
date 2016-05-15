using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    public class AtomicIntegerArray : IAtomicRefArray<int>
    {
        private readonly int[] _data;
        private readonly MemoryOrder _order;

        public AtomicIntegerArray(int[] source, MemoryOrder order = MemoryOrder.SeqCst)
            : this(source.Length)
        {
            source.CopyTo(_data, 0);
        }

        public AtomicIntegerArray(int length, MemoryOrder order = MemoryOrder.SeqCst)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order.ToString()));

            _data = new int[length];
            _order = order;
        }

        public void Store(int index, ref int value, MemoryOrder order)
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
#else
                    this._data[index] = value;
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

        public int this[int i]
        {
            get { return Load(i, _order); }
            set { Store(i, ref value, _order); }
        }

        public void Store(int index, int value, MemoryOrder order)
        {
            Store(index, ref value, order);
        }

        public int Load(int index, MemoryOrder order)
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

        public bool IsLockFree
        {
            get { return true; }
        }

        public int CompareExchange(int index, int value, int comparand)
        {
            return Interlocked.CompareExchange(ref this._data[index], value, comparand);
        }
    }
}
