using System.Runtime.CompilerServices;

namespace System.Threading.Atomics
{
    public static class Platform
    {
        /// <summary>
        /// Reads value from provided <paramref name="location"/> without any synchronization
        /// </summary>
        /// <typeparam name="T">The reference (<paramref name="location"/>) type</typeparam>
        /// <param name="location">The location to read</param>
        /// <returns>Value stored at provided <paramref name="location"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(ref T location)
        {
            return location;
        }

        /// <summary>
        /// Reads value from provided <paramref name="location"/> with acquire semantics
        /// </summary>
        /// <typeparam name="T">The reference (<paramref name="location"/>) type</typeparam>
        /// <param name="location">The location to read</param>
        /// <returns>Value stored at provided <paramref name="location"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadAcquire<T>(ref T location)
        {
#if ARM_CPU
            var tmp = location;
            Interlocked.MemoryBarrier();
#endif
            return location;
        }

        /// <summary>
        /// Reads value from provided <paramref name="location"/> with sequential consistent semantics
        /// </summary>
        /// <typeparam name="T">The reference (<paramref name="location"/>) type</typeparam>
        /// <param name="location">The location to read</param>
        /// <returns>Value stored at provided <paramref name="location"/></returns>
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

        /// <summary>
        /// Writes <paramref name="value"/> to provided <paramref name="location"/>
        /// </summary>
        /// <typeparam name="T">The reference (<paramref name="location"/>) type</typeparam>
        /// <param name="location">The location to store the <paramref name="value"/></param>
        /// <param name="value">The value to be written to provided <paramref name="location"/></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(ref T location, ref T value)
        {
            location = value;
        }

        /// <summary>
        /// Writes <paramref name="value"/> to provided <paramref name="location"/> with release semantics
        /// </summary>
        /// <typeparam name="T">The reference (<paramref name="location"/>) type</typeparam>
        /// <param name="location">The location to store the <paramref name="value"/></param>
        /// <param name="value">The value to be written to provided <paramref name="location"/></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRelease<T>(ref T location, ref T value)
        {
#if ARM_CPU
            var tmp = value;
            Interlocked.MemoryBarrier();
            location = tmp;
#else
            location = value;
#endif
        }

        /// <summary>
        /// Writes <paramref name="value"/> to provided <paramref name="location"/> with sequential consistent semantics
        /// </summary>
        /// <typeparam name="T">The reference (<paramref name="location"/>) type</typeparam>
        /// <param name="location">The location to store the <paramref name="value"/></param>
        /// <param name="value">The value to be written to provided <paramref name="location"/></param>
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