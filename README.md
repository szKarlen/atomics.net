# atomics.net

[![Build status](https://ci.appveyor.com/api/projects/status/wnh7fat3oqas0wer?svg=true)](https://ci.appveyor.com/project/szKarlen/atomics-net) [![NuGet](https://img.shields.io/nuget/v/System.Threading.Atomics.svg?style=flat)](http://www.nuget.org/profiles/Karlen)

This package enables .NET projects to use atomic primitives.

Design and implementation
-------

Project aims to be very close to C++ 11 standard atomics by design and usage. For example, The [memory order](http://en.cppreference.com/w/cpp/atomic/memory_order) flag could be provided to primitives.

Although the library is a PCL itself, the minimum required version of .NET - 4.5. But you can compile for .NET 4.0 and earlier. The Itanium-related stuff (volatile reads with proper memory barriers usages, etc.) will be present (see [docs](Documentation/memorymodel101.md)).

For ECMA MM implementations of CLI on ARM architecture the conditional compilation is support by using ARM_CPU directive.

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
Read/writes operations on references are provided by `AtomicReference<T>`.
The `Atomic<T>` class should be used for structs (i.e. value types), including (`char`, `byte`, etc.).

`AtomicInteger` and `AtomicLong` classes has support for `+, -, *, /, ++, --, +=, -=, *=, /=` operators with atomicity guarantees.

All primitives implement the implicit conversion operator overloads with atomic access.

Integers ranging from 8 to 64 bit are supported as well as unsigned ones.

Sample usage
-------

Here is the basic setup and usage of atomic primitives.

``` csharp
using System;

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
    
    public void PrintCounter()
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

Lock-free stack 101
-------

It is very straightforward to implement lock-free stack:
``` csharp
public class AtomicStack<T>
{
    private AtomicReference<StackNode<T>> _head = new AtomicReference<StackNode<T>>();
    private readonly AtomicBoolean _isEmpty = new AtomicBoolean(true);

    public void Push(T item)
    {
        m_head.Set(stackNode =>
        {
            StackNode<T> node = new StackNode<T>(item);
            node._next = _head;

            IsEmpty = false;
            
            return node;
        });
    }

    public T Pop()
    {
        if (IsEmpty)
            throw new InvalidOperationException();
            
        return _head.Set(stackNode =>
        {
            IsEmpty = stackNode._next == null;
            return stackNode._next;
        })._value;
    }

    public bool IsEmpty
    {
        get { return _isEmpty; }
        private set { _isEmpty.Value = value; }
    }

    class StackNode<T>
    {
        internal T _value;
        internal StackNode<T> _next;
        internal StackNode(T val) { _value = val; }
    }
}
```

and usage:
``` csharp
AtomicStack<int> stack = new AtomicStack<int>();

Parallel.For(0, 2000, stack.Push);

while (!stack.IsEmpty)
{
    Console.WriteLine(stack.Pop());
}
```

Console should output numbers ranging from 1999 to 0.

For more details about `AtomicStack<T>` example above, please refer [docs](Documentation/lockfreestack101.md).

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

[Package's page](https://www.nuget.org/packages/System.Threading.Atomics)

Command: `PM> Install-Package System.Threading.Atomics -Pre`

License
-------

atomics.net is licensed under the [BSD license](LICENSE).
