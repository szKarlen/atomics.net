using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    class _32BitPod<T> : IAtomicsOperator<T> where T : struct, IEquatable<T>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable")]
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct PodOffset
        {
            [FieldOffset(0)]
            public int Int32Location;
            [FieldOffset(0)]
            public T Location;

            [FieldOffset(4)]
            public int Int32Value;
            [FieldOffset(4)]
            public T Value;

            [FieldOffset(8)]
            public int Int32Comparand;
            [FieldOffset(8)]
            public T Comparand;

            [FieldOffset(12)]
            public int Int32Result;
            [FieldOffset(12)]
            public T Result;

            public T CompareExchange(ref T location1, ref T value, ref T comparand)
            {
                this.Location = location1;
                this.Value = value;
                this.Comparand = comparand;

                this.Int32Result = Interlocked.CompareExchange(ref this.Int32Location, this.Int32Value, this.Int32Comparand);
                return this.Result;
            }
        }

        public _32BitPod()
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
            return Marshal.SizeOf(typeof (TType)) == 4;
        }
    }
}
