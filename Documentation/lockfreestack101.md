# Lock-free stack 101

This doc aims to provide some examples and ideas of how to implement LIFO, FIFO lock-free stack using atomics.net.
For more detailed description of terms, please refer to [glossary](glossary.md).

Acquire/Release through CAS approach
-------

Usually **compare-and-swap (CAS)** is used in lock-free algorithms for locks, interlocked operations implementations, etc., especially `compare_exchange_weak` variation.
Provided by the .NET Framework [`Interlocked.CompareExchange`](https://msdn.microsoft.com/ru-ru/library/system.threading.interlocked.compareexchange(v=vs.110).aspx) method is the C++ [`compare_and_exchange_strong`](http://en.cppreference.com/w/cpp/atomic/atomic/compare_exchange) analog. The `compare_exchange_weak` is not supported.

Current implementation of atomics.net uses CAS approach for lock-free atomic operations.

So any atomic primitive does use for get/set operations CAS in Acquire/Release mode.

LIFO container aka Stack
-------

As an initial example of a lock-free stack implementation lets look at the followig example (borrowed from [Joe Duffy
](http://msdn.microsoft.com/msdnmag/issues/07/05/CLRInsideOut/default.aspx) article at MSDN Magazine from May 2007):
``` csharp
public class LockFreeStack<T> {
    private volatile StackNode<T> m_head;

    public void Push(T item) {
        StackNode<T> node = new StackNode<T>(item);
        StackNode<T> head;
        do {
            head = m_head;
            node.m_next = head;
        } while (m_head != head || Interlocked.CompareExchange(
                ref m_head, node, head) != head);
    }

    public T Pop() {
        StackNode<T> head;
        SpinWait s = new SpinWait();

        while (true) {
            StackNode<T> next;
            do {
                head = m_head;
                if (head == null) goto emptySpin;
                next = head.m_next;
            } while (m_head != head || Interlocked.CompareExchange(
                    ref m_head, next, head) != head);
            break;

        emptySpin:
            s.Spin();
        }

        return head.m_value;
    }
}

class StackNode<T> {
    internal T m_value;
    internal StackNode<T> m_next;
    internal StackNode(T val) { m_value = val; }
}
```
This stack is very simple with forever loop waiting for pushed items.
Quote from original article:
>Also, we’ve taken a rather naïve approach to the case in which the stack has become empty. We just spin forever, waiting for a new item to be pushed. It’s straightforward to rewrite Pop into a non-waiting TryPop method, and a bit more complex to make use of events for waiting. Both are important features and left as exercises for the motivated reader.

Good, lets rewrite with IsEmpty property support for further iteration through collection:
``` csharp
public class LockFreeStack<T>
{
    private StackNode<T> m_head;
    private volatile bool _isEmpty;

    public LockFreeStack()
    {
        IsEmpty = true;
    }

    public void Push(T item)
    {
        StackNode<T> node = new StackNode<T>(item);
        StackNode<T> head;
        do
        {
            head = m_head;
            node.m_next = head;
        } while (m_head != head || Interlocked.CompareExchange(
                ref m_head, node, head) != head);

        IsEmpty = false;
    }

    public T Pop()
    {
        if (IsEmpty)
            throw new InvalidOperationException();

        StackNode<T> head = null;

        while (true)
        {
            StackNode<T> next;
            do
            {
                head = m_head;
                if (head.m_next == null)
                {
                    IsEmpty = true;
                    break;
                }
                next = head.m_next;
                IsEmpty = false;
            } while (m_head != head || Interlocked.CompareExchange(
                    ref m_head, next, head) != head);

            break;
        }

        return head.m_value;
    }

    public bool IsEmpty
    {
        get { return _isEmpty; }
        private set { _isEmpty = value; }
    }
}
```

and usage example with stress-test:
``` csharp
LockFreeStack<int> stack = new LockFreeStack<int>();

Parallel.For(0, 2000, stack.Push);

while (!stack.IsEmpty)
{
    Console.WriteLine(stack.Pop());
}
```

Console should output numbers ranging from 1999 to 0.

For now, lets update to atomics.net usage:
``` csharp
public class AtomicStack<T>
{
    private AtomicReference<StackNode<T>> m_head = new AtomicReference<StackNode<T>>();
    private readonly AtomicBoolean _isEmpty = new AtomicBoolean(true);

    public void Push(T item)
    {
        IsEmpty = m_head.Set(stackNode =>
        {
            StackNode<T> node = new StackNode<T>(item);
            node.m_next = m_head;

            return node;
        }) != null;
    }

    public T Pop()
    {
        if (IsEmpty)
            throw new InvalidOperationException();

        StackNode<T> head = m_head.Set(stackNode =>
        {
            head = m_head;
            return head.m_next;
        });

        IsEmpty = head.m_next == null;

        return head.m_value;
    }

    public bool IsEmpty
    {
        get { return _isEmpty; }
        private set { _isEmpty.Value = value; }
    }

    class StackNode<T>
    {
        internal T m_value;
        internal StackNode<T> m_next;
        internal StackNode(T val) { m_value = val; }
    }
}
```

Run stress-test and get equal output.

Final notes
-------

In last example (i.e. `AtomicStack<T>`) the method Push() may look a little bit confusing
``` csharp
public void Push(T item)
{
    IsEmpty = m_head.Set(stackNode =>
    {
        StackNode<T> node = new StackNode<T>(item);
        node.m_next = m_head;

        return node;
    }) != null;
}
```
At line 8 (`return node;`) we return the setter value, while at line 3 (`IsEmpty = m_head.Set(stackNode =>`) the return value is the previous value of `AtomicReference<StackNode<T>>.Value`;