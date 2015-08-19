using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    static class MemoryModelHelpers
    {
        public static bool IsAcquireRelease(this MemoryOrder order)
        {
            return order >= MemoryOrder.Release && order <= MemoryOrder.AcqRel;
        }

        public static bool IsSpported(this MemoryOrder order)
        {
            return !(order < MemoryOrder.Acquire);
        }

        public static int ToInt32(this bool target)
        {
            return target ? 1 : 0;
        }
    }
}
