using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics.Interop
{
    public enum ProcessorArchitecture : ushort
    {
        /// <summary>
        /// x86
        /// </summary>
        INTEL = 0,

        /// <summary>
        /// ARM
        /// </summary>
        ARM = 5,

        /// <summary>
        /// Intel Itanium-based
        /// </summary>
        IA64 = 6,

        /// <summary>
        /// x64 (AMD or Intel)
        /// </summary>
        AMD64 = 9,
        
        /// <summary>
        /// Unknown architecture
        /// </summary>
        UNKNOWN = 0xffff
    }

    public class SystemInfo
    {
        public ProcessorArchitecture ProcessorArchitecture;
        public ushort ProcessorArchitectureId;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct _SYSTEM_INFO
    {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public IntPtr lpMinimumApplicationAddress;
        public IntPtr lpMaximumApplicationAddress;
        public UIntPtr dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
    };

    static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        private static extern void GetNativeSystemInfo(ref _SYSTEM_INFO lpSystemInfo);

        public static ProcessorArchitecture GetProcessorArchitecture()
        {
            try
            {
                var sysInfo = new _SYSTEM_INFO();
                GetNativeSystemInfo(ref sysInfo);
                return Enum.IsDefined(typeof (ProcessorArchitecture), sysInfo.wProcessorArchitecture)
                    ? (ProcessorArchitecture) sysInfo.wProcessorArchitecture
                    : ProcessorArchitecture.UNKNOWN;
            }
            catch (Exception e) // suppress EntryPointNotFoundException
            {
                return ProcessorArchitecture.UNKNOWN;
            }
        }

        public static bool HasStrongMM()
        {
            var cpuId = GetProcessorArchitecture();
            return cpuId == ProcessorArchitecture.AMD64 || cpuId == ProcessorArchitecture.INTEL;
        }
    }
}
