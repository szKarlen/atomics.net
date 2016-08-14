# atomics.net

[![Build status](https://ci.appveyor.com/api/projects/status/wnh7fat3oqas0wer?svg=true)](https://ci.appveyor.com/project/szKarlen/atomics-net) [![NuGet](https://img.shields.io/nuget/v/System.Threading.Atomics.svg?style=flat)](http://www.nuget.org/packages/System.Threading.Atomics/)

This package enables .NET projects to use atomic primitives.

Design and implementation
-------

Project aims to be very close to C++ 11 standard atomics by design and usage. For example, the [memory order](http://en.cppreference.com/w/cpp/atomic/memory_order) semantics is supported.

Although the library is a PCL itself, the minimum required version of .NET is 4.5. It is possible to compile and use for .NET 4.0 and earlier. ARM-related stuff (volatile reads with proper memory barriers usages, etc.) will be present by using ARM_CPU directive (see [docs](Documentation/memorymodel101.md)).

The default memory order semantics for the library's primitives (like `Atomic<T>`, etc.) is `MemoryOrder.SeqCst`, whereas `AtomicReference<T>` uses `MemoryOrder.AcqRel`, which fits very well with CAS approach and CLR 2.0 memory model.

The option for sequential consistency (i.e. `SeqCst`) is implemented by using intrinsic functions (with compilation to proper CPU instruction) or a combination of Acquire/Release with sequential order emulation through exclusion locks, when atomic reads/writes to particular POD are not supported by HW.

Atomic primitives
-------

* `Atomic<T>`
* `AtomicReference<T>`
* `AtomicInteger`
* `AtomicLong`
* `AtomicBoolean`
* `AtomicReferenceArray`
* `AtomicIntegerArray`
* `AtomicLongArray`

Supported types and operations
-------
Reads/writes operations for reference types are provided by `AtomicReference<T>`.
The `Atomic<T>` type should be used for structs (i.e. value types), including (`char`, `byte`, etc.).

`AtomicInteger` and `AtomicLong` types have support for `+, -, *, /, ++, --` operators with atomicity guarantees.

All primitives implement the implicit conversion operator overload with atomic access.

Integers ranging from 8 to 64 bits are supported as well as unsigned ones.

False Sharing
-------

`AtomicInteger` and `AtomicLong` types has support for memory alignment alongside modern CPU's cache lines. Use flag `align` in constructors of either `Atomic<T>`, `AtomicInteger`, `AtomicLong` or `AtomicBoolean`. Only specializations of `Atomic<T>` with `Int32`, `Int64` and `Boolean` have effect.

Sample usage
-------

Here is the basic setup and usage of atomic primitives.

``` csharp
using System;

class Counter
{
    private AtomicInteger _value;
    private readonly bool _isReadOnly;
    
    public Counter(int initialValue = 0, bool isReadOnly = false)
    {
        /*
         * _value = new AtomicInteger(align: true)
         * for false sharing prevention, otherwise as shown below
         */
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

`Atomic<T>` with `Int32`, `Int64` and `Boolean` specialization fallbacks to `AtomicInteger`, `AtomicLong` and `AtomicBoolean` usage as internal storage respectively.

The memory ordering flag as well as alignment transfers to internal storage.

Lock-free stack 101
-------

It is very straightforward to implement lock-free stack:
``` csharp
public class AtomicStack<T>
{
    private AtomicReference<StackNode<T>> _headNode = new AtomicReference<StackNode<T>>();

    public void Push(T item)
    {
        _headNode.Set((stackNode, data) =>
        {
            StackNode<T> node = new StackNode<T>(data);
            node._next = stackNode;

            return node;
        }, item);
    }

    public T Pop()
    {
        if (IsEmpty)
            throw new InvalidOperationException();

        return _headNode.Set(stackNode => stackNode._next)._value;
    }

    public bool IsEmpty
    {
        get { return _headNode.Load(MemoryOrder.Acquire) == null; }
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

Parallel.For(0, 100000, stack.Push);

var thread = new Thread(() => Parallel.For(0, 50000, index => stack.Pop()));
thread.IsBackground = true;
thread.Start();

int i = 0;
while (!stack.IsEmpty)
{
    stack.Pop();
    i++;
}

Console.WriteLine("Pushed: {0};", i); // should print 50000
```

For more details about `AtomicStack<T>` example above, please refer [docs](Documentation/lockfreestack101.md).

CAS notes
-------
Usually **compare-and-swap (CAS)** is used in lock-free algorithms to maintain thread-safety, while avoiding locks. Especially often the `compare_exchange_weak` variation is used.
Provided by the .NET Framework [`Interlocked.CompareExchange`](https://msdn.microsoft.com/ru-ru/library/system.threading.interlocked.compareexchange(v=vs.110).aspx) method is the C++ [`compare_and_exchange_strong`](http://en.cppreference.com/w/cpp/atomic/atomic/compare_exchange) analog. The `compare_exchange_weak` is not supported.

Current implementation of atomics.net uses CAS approach for lock-free atomic operations (the `Atomic<T>.Value` property uses CAS for setter in Acquire/Release mode.

Contributing
-------

Feel free to fork and create pull-requests if you have any kind of enhancements and/or bug fixes.

NuGet
-------

[Package's page](https://www.nuget.org/packages/System.Threading.Atomics)

Command: `PM> Install-Package System.Threading.Atomics`

License
-------

atomics.net is licensed under the [BSD license](LICENSE).
