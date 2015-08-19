using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    /// <summary>
    /// Specifies how regular, non-atomic memory accesses are to be ordered around an atomic operation
    /// </summary>
    /// <remarks>The Default behavior for <see cref="Atomic{T}"/>, excluding <see cref="AtomicInteger"/>, <see cref="AtomicLong"/> and <see cref="AtomicBoolean"/>, which are using <see cref="AcqRel"/> semantics.</remarks>
    public enum MemoryOrder
    {
        /// <summary>
        /// Relaxed operation: there are no synchronization or ordering constraints, only atomicity is required of this operation.
        /// </summary>
        /// <remarks>CLR JIT on Itanium does emit ST.REL for non-volatile writes.</remarks>
        [Obsolete]
        Relaxed,
        
        /// <summary>
        /// A load operation with this memory order performs a consume operation on the affected memory location: no reads in the current thread dependent on the value currently loaded can be reordered before this load. This ensures that writes to data-dependent variables in other threads that release the same atomic variable are visible in the current thread. On most platforms, this affects compiler optimizations only.
        /// </summary>
        /// <remarks>Does not have any effect. DEC Alpha alike architectures are not supported</remarks>
        [Obsolete]
        Consume,

        /// <summary>
        /// A load operation with this memory order performs the acquire operation on the affected memory location: no memory accesses in the current thread can be reordered before this load. This ensures that all writes in other threads that release the same atomic variable are visible in the current thread.
        /// </summary>
        /// <remarks>Current atomics implementation does use only <see cref="AcqRel"/> semantics. Usage of this flag fallbacks to <see cref="AcqRel"/></remarks>
        Acquire,

        /// <summary>
        /// A store operation with this memory order performs the release operation: no memory accesses in the current thread can be reordered after this store. This ensures that all writes in the current thread are visible in other threads that acquire or the same atomic variable and writes that carry a dependency into the atomic variable become visible in other threads that consume the same atomic.
        /// </summary>
        /// <remarks>Current atomics implementation does use only <see cref="AcqRel"/> semantics. Usage of this flag fallbacks to <see cref="AcqRel"/></remarks>
        Release,
        
        /// <summary>
        /// A read-modify-write operation with this memory order is both an acquire operation and a release operation. No memory accesses in the current thread can be reordered before this load, and no memory accesses in the current thread can be reordered after this store. It is ensured that all writes in another threads that release the same atomic variable are visible before the modification and the modification is visible in other threads that acquire the same atomic variable.
        /// </summary>
        AcqRel,

        /// <summary>
        /// Same as <see cref="AcqRel"/>, plus a single total order exists in which all threads observe all modifications (see below) in the same order.
        /// </summary>
        /// <remarks>Default behavior for <see cref="Atomic{T}"/>, excluding <see cref="AtomicInteger"/>, <see cref="AtomicLong"/> and <see cref="AtomicBoolean"/>, which are using <see cref="AcqRel"/> semantics</remarks>
        SeqCst
    }
}
