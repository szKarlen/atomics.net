**Sequential Consistency** (definition of sequential consistency by Leslie Lamport) - the result of any execution is the same as if the operations of all the processors were executed in some sequential order, and the operations of each individual processor appear in this sequence in the order specified by its program. 

**Acquire/Release** (or Barrier/Fence, or Happens-Before) - memory model semantics, where an acquire operation means no loads or stores may move before it, and a release operation means no loads or stores may move after it.

**x86** - a family of backward compatible instruction set architectures[a] based on the Intel 8086 CPU and its Intel 8088 variant.

**x86-64** (also known as x64, x86_64 and AMD64) - the 64-bit version of the x86 instruction set.

**Itanium** - a family of 64-bit Intel microprocessors that implement the Intel Itanium architecture (formerly called IA-64). Intel markets the processors for enterprise servers and high-performance computing systems. The Itanium architecture originated at Hewlett-Packard (HP), and was later jointly developed by HP and Intel.

**ECMA CLI Memory Model** - Section 12.6 of Partition I of the ECMA CLI specification, which explains explains the alignment rules, byte ordering, the atomicity of loads and stores, volatile semantics, locking behavior, etc.

**CLR** - implementation of the Virtual Execution System (VES) as defined in the ECMA Common Language Infrastructure (CLI) standard.

**CLI** - an open specification developed by Microsoft and standardized by ISO[1] and ECMA[2] that describes the executable code and runtime environment. The specification defines an environment that allows multiple high-level languages to be used on different computer platforms without being rewritten for specific architectures. The .NET Framework os an implementations of the CLI.

**Fence** - is a type of barrier instruction that causes a central processing unit (CPU) or compiler to enforce an ordering constraint on memory operations issued before and after the barrier instruction.

**Memory barrier** - a class of instructions which cause a processor to enforce an ordering constraint on memory operations issued before and after the barrier instruction. A barrier can also be a high-level programming language statement which prevents the compiler from reordering other operations over the barrier statement during optimization passes. Such statements can potentially generate processor barrier instructions. Different classes of barrier exist and may apply to a specific set of operations only.