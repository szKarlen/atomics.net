using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    class _64BitPod<T> : IAtomicsOperator<T> where T : struct, IEquatable<T>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable")]
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PodOffset
        {
            [FieldOffset(0)]
            public long Int64Location;
            [FieldOffset(0)]
            public T Location;

            [FieldOffset(8)]
            public long Int64Value;
            [FieldOffset(8)]
            public T Value;

            [FieldOffset(16)]
            public long Int64Comparand;
            [FieldOffset(16)]
            public T Comparand;

            [FieldOffset(24)]
            public long Int64Result;
            [FieldOffset(24)]
            public T Result;

            public T CompareExchange(ref T location1, ref T value, ref T comparand)
            {
                this.Location = location1;
                this.Value = value;
                this.Comparand = comparand;

                this.Int64Result = Interlocked.CompareExchange(ref this.Int64Location, this.Int64Value, this.Int64Comparand);
                return this.Result;
            }
        }

        public _64BitPod()
        {
            
        }

        public T CompareExchange(ref T location1, T value, T comparand)
        {
            PodOffset pod = new PodOffset();
            return pod.CompareExchange(ref location1, ref value, ref comparand);
        }

        public T Read(ref T location1)
        {
            Interlocked.MemoryBarrier();
            return location1;
        }

        bool IAtomicsOperator<T>.Supports<TType>()
        {
            return Marshal.SizeOf(typeof(TType)) == 8;
        }
    }
}
