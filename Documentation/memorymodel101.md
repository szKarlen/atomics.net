# Memory Model 101

This doc aims to provide some ideas behind particular implementation of the atomics.net on top of .NET.
For more detailed description of terms, please refer to [glossary](glossary.md).

atomics.net design and implementation
-------

Project aims to be very close to C++ 11 standard atomics by design and usage. For example, the [memory order](http://en.cppreference.com/w/cpp/atomic/memory_order) semantics is supported.

Although the library is a PCL itself, the minimum required version of .NET is 4.5. It is possible to compile and use for .NET 4.0 and earlier. ARM-related stuff (volatile reads with proper memory barriers usages, etc.) will be present by using ARM_CPU directive (see [docs](Documentation/memorymodel101.md)).

The default memory order semantics for the library's primitives (like `Atomic<T>`, etc.) is `MemoryOrder.SeqCst`, whereas `AtomicReference<T>` uses `MemoryOrder.AcqRel`, which fits very well with CAS approach and CLR 2.0 memory model.

The option for sequential consistency (i.e. `SeqCst`) is implemented by using intrinsic functions (with compilation to proper CPU instruction) or a combination of Acquire/Release with sequential order emulation through exclusion locks, when atomic reads/writes to particular POD are not supported by HW.

Specifying Acquire only or Release only flag falls back to full Acquire/Release semantics for get/set operations or combinations of.

x86-x64 memory model vs ECMA
-------

Starting from CLR 2.0 the memory model had become more stricter than ECMA CLI Memory Model standard defines and very close to x86 behavior.

The main changes are (notes are taken from this [article](http://msdn.microsoft.com/msdnmag/issues/05/10/MemoryModels/) by Vance Morrison):

* (ECMA) Reads and writes cannot move before a volatile read. 
* (ECMA) Reads and writes cannot move after a volatile write. 
* (x86) A write can only move later in time.
* (x86) A write cannot move past another write from the same thread.
* (x86) A write cannot move past a read from the same thread to the same location.
* (x86) A read can only move by going later in time to stay after a write to keep from breaking rule 3 as that write moves later in time.

Summarizing the info above we are dealing with the following rules (taken from this [article](http://joeduffyblog.com/2007/11/10/clr-20-memory-model/) by Joe Duffy):

1. Data dependence among loads and stores is never violated.
2. All stores have release semantics, i.e. no load or store may move after one.
3. All volatile loads are acquire, i.e. no load or store may move before one.
4. No loads and stores may ever cross a full-barrier (e.g. Thread.MemoryBarrier, lock acquire, Interlocked.Exchange, Interlocked.CompareExchange, etc.).
5. Loads and stores to the heap may never be introduced.
6. Loads and stores may only be deleted when coalescing adjacent loads and stores from/to the same location.

and final [note](http://joeduffyblog.com/2007/11/10/clr-20-memory-model/):

>"...Note that by this definition, non-volatile loads are not required to have any sort of barrier associated with them.  So loads may be freely reordered, and writes may move after them (though not before, due to Rule 2).  With this model, the only true case where you’d truly need the strength of a full-barrier provided by Rule 4 is to prevent reordering in the case where a store is followed by a volatile load.  Without the barrier, the instructions may reorder..."

x86-x64 Memory Reorderings
-------

There is one place when the reordering will occur on x86-64 systems ([source](https://msdn.microsoft.com/en-us/magazine/jj883956.aspx) by Igor Ostrovsky):
>...Even though the x86-x64 processor provides fairly strong ordering guarantees, a particular kind of hardware reordering still happens.
The x86-x64 processor will not reorder two writes, nor will it reorder two reads. However, the one (and only) possible reordering effect is that when a processor writes a value, that value will not be made immediately available to other processors...

>...The x86 processor has a fairly strong memory model, and **the only source of reordering at the hardware level is the store buffer**. The store buffer can cause a write to get reordered with a subsequent read (store-load reordering).<br/>
Also, certain compiler optimizations can result in reordering of memory operations. Notably, if several reads access the same memory location, the compiler might choose to perform the read only once and keep the value in a register for subsequent reads.<br/>
One interesting piece of trivia is that the **C# volatile semantics closely match the hardware reordering guarantees made by x86-x64 hardware**. As a result, reads and writes of volatile fields require no special instructions on the x86: Ordinary reads and writes (for example, using the MOV instruction) are sufficient. Of course, your code shouldn’t depend on these implementation details because they vary among hardware architectures and possibly .NET versions...

.NET 4.5 implementation
-------

As we saw above the CLR 2.0 memory model is very close to x86 systems. Among other popular (or some kind of) platforms are Itanium and ARM.

Starting from .NET 4.5 support for Itanium was [dropped](https://msdn.microsoft.com/en-us/library/8z6watww.aspx).

This means that there is no need for additional explicit barrier/fences to be used for processor cache coherence support. X86-64 do this by design.

The only situation about acquire/release semantics for .NET was the following [behavior](#itanium): **_CLR JIT on Itanium does emit ST.REL for non-volatile writes_**, because Itanium distinguishes between an ordinary load (LD) and load-acquire (LD.ACQ), and an ordinary store (ST) and store-release (ST.REL) ([source](https://msdn.microsoft.com/en-us/magazine/jj883956.aspx)).

This means, that all writes are volatile, while reads - not and requires to use memory barrier for acquire.