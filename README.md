# atomics.net

[![Build status](https://ci.appveyor.com/api/projects/status/wnh7fat3oqas0wer?svg=true)](https://ci.appveyor.com/project/szKarlen/atomics-net)

This package enables .NET projects to use atomic primitives.

Design and implementation
-------

Project aims to be very close to C++ 11 standard atomics by design and usage. For example, The [memory order](http://en.cppreference.com/w/cpp/atomic/memory_order) flag could be provided to primitives.

Although the library is a PCL itself, the minimum required version of .NET - 4.5. But you can compile for .NET 4.0 and earlier. The Itanium-related stuff (reorderings, fences and barrier usages, etc.) will be present (see [docs](Documentation/memorymodel101.md)).

The default memory semantics for library's primitives is Acquire/Release, which fits very well with .NET Framework and CLR 2.0 memory model.

The option for sequential consistency is implemented as a combination of Acquire/Release with sequential order emulation by mutual exclusion locks.

Specifing Acquire only or Release only flag falls back to full Acquire/Release semantics for get/set operations or combinations of.

Atomic primitives
-------

* `Atomic<T>`
* `AtomicReference<T>`
* `AtomicInteger`
* `AtomicLong`
* `AtomicBoolean`

Supported types and operations
-------
The reference types pointers read/writes are provided by `AtomicReference<T>`.
The `Atomic<T>` class should be used for structs (i.e. value types).

`AtomicInteger` and `AtomicLong` classes has support for `+, -, *, /, ++, --, +=, -=, *=, *=` operators support with atomicity guarantees.

All primitives implement the implicit conversion operator overloads with atomic accesses.

Integers support is ranging from 8 to 64 bit with unsigned ones as well.

Sample usage
-------

Here is the basic setup and usage of atomic primitives.

``` csharp
using System;
using System.IO;

class Counter
{
    private AtomicInteger _value;
    private readonly AtomicBoolean _isReadOnly;
    
    public Counter(int initialValue = 0, bool isReadOnly = false)
    {
        _value = initialValue;
        _isReadOnly = isReadOnly;
    }
    
    public void Increment(int value)
    {
        if (!_isReadOnly)
            _value++;
    }
    
    public void Show()
    {
        Console.WriteLine(_value); // Console.WriteLine(int) overload will be used
    }
}
```

Notes for usage
-------

`Atomic<T>` with `Int32`, `Int64` and `Boolean` specialization falls back to using `AtomicInteger`, `AtomicLong` and `AtomicBoolean` as internal storage respectively.

The only difference for performance is that `AtomicInteger`, `AtomicLong` and `AtomicBoolean` are using Acquire/Release semantics by default, while default memory order flag for `Atomic<T>` is sequential consistency, which trasfers to internal storage as well.

Consider the following:

``` csharp
var atomicBool = new Atomic<bool>(false);
// is equal to
var atomicBoolSeqCst = new AtomicBoolean(false, MemoryOrder.SeqCst);
```
and
``` csharp
var atomicBoolAcqRel = new Atomic<bool>(false, MemoryOrder.AcqRel);
// is equal to
var atomicBool = new AtomicBoolean(false);
```

The above example applies to `Atomic<int>` with `AtomicInteger`, and `Atomic<long>` with `AtomicLong`.

CAS notes
-------
Usually **compare-and-swap (CAS)** is used in lock-free algorithms for locks, interlocked operations implementations, etc., especially `compare_exchange_weak` variation.
Provided by the .NET Framework [`Interlocked.CompareExchange`](https://msdn.microsoft.com/ru-ru/library/system.threading.interlocked.compareexchange(v=vs.110).aspx) method is the C++ [`compare_and_exchange_strong`](http://en.cppreference.com/w/cpp/atomic/atomic/compare_exchange) analog. The `compare_exchange_weak` is not supported.
Current implementation of atomics.net uses CAS approach for lock-free atomic operations.

Contributing
-------

Feel free to fork and create pull-requests if you have any kind of enhancements and/or bug fixes.

NuGet
-------

Coming soon

License
-------

atomics.net is licensed under the [BSD license](LICENSE).