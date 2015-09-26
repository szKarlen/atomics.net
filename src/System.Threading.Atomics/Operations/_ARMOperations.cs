using System.Runtime.CompilerServices;

namespace System.Threading.Atomics
{
    class _ARMOperations : IOperations
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(ref T location)
        {
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadAcquire<T>(ref T location)
        {
            var tmp = location;
            Interlocked.MemoryBarrier();
            return tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadSeqCst<T>(ref T location)
        {
            Interlocked.MemoryBarrier();
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
            Interlocked.MemoryBarrier();
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSeqCst<T>(ref T location, ref T value)
        {
            Interlocked.MemoryBarrier();
            var tmp = value;
            Interlocked.MemoryBarrier();
            location = tmp;
        }
    }
}