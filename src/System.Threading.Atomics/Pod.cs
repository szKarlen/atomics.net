using System.Runtime.InteropServices;

namespace System.Threading.Atomics
{
    static class Pod<T>
    {
        private static readonly int PodSize = Marshal.SizeOf(typeof(T));

        public static int Size
        {
            get { return PodSize; }
        }

        public static bool AtomicRWSupported()
        {
            var podType = typeof(T);
            return podType == typeof(bool)
                || podType == typeof(char)
                || podType == typeof(byte)
                || podType == typeof(sbyte)
                || podType == typeof(short)
                || podType == typeof(ushort)
                || podType == typeof(int)
                || podType == typeof(uint)
                || podType == typeof(float)
                || (IntPtr.Size == 8 && (podType == typeof(double) || podType == typeof(long)));
        }
    }
}
