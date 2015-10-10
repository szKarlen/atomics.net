namespace System.Threading.Atomics
{
    interface IAtomic<T> : IAtomicOperators<T> where T : struct
    {
        /// <summary>
        /// Gets or sets atomically the underlying value
        /// </summary>
        T Value { get; set; }

        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        void Store(T value, MemoryOrder order);

        /// <summary>
        /// Gets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        T Load(MemoryOrder order);

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        bool IsLockFree { get; }
    }

    interface IAtomicRef<T> : IAtomic<T> where T : struct
    {
        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        void Store(ref T value, MemoryOrder order);
    }
}