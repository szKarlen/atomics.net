using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    class _x86Operations : IOperations
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(ref T location)
        {
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadAcquire<T>(ref T location)
        {
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadSeqCst<T>(ref T location)
        {
            var tmp = location;
            Interlocked.MemoryBarrier();
            return tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(ref T location, ref T value)
        {
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRelease<T>(ref T location, ref T value)
        {
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSeqCst<T>(ref T location, ref T value)
        {
            Interlocked.MemoryBarrier();
            location = value;
        }
    }
}
