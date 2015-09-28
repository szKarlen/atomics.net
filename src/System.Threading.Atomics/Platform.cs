using System.Runtime.CompilerServices;

namespace System.Threading.Atomics
{
    public static class Platform
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(ref T location)
        {
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadAcquire<T>(ref T location)
        {
#if ARM_CPU
            var tmp = location;
            Interlocked.MemoryBarrier();
#endif
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadSeqCst<T>(ref T location)
        {
#if ARM_CPU
            Interlocked.MemoryBarrier();
#endif
            var tmp = location;
            Interlocked.MemoryBarrier();
            return tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(ref T location, ref T value)
        {
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRelease<T>(ref T location, ref T value)
        {
#if ARM_CPU
            var tmp = location;
#endif
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSeqCst<T>(ref T location, ref T value)
        {
#if ARM_CPU
            Interlocked.MemoryBarrier();
            var tmp = value;
            Interlocked.MemoryBarrier();
            location = tmp;
#else
            Interlocked.MemoryBarrier();
            location = value;
#endif

        }
    }
}