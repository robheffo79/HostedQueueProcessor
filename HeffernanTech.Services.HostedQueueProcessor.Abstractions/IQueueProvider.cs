namespace HeffernanTech.Services.HostedQueueProcessor.Abstractions
{
	/// <summary>
	/// Defines a provider that manages a queue of items.
	/// </summary>
	/// <typeparam name="T">The type of elements in the queue.</typeparam>
	public interface IQueueProvider<T>
	{
		/// <summary>
		/// Gets the <see cref="AutoResetEvent"/> that is used to signal when an item is enqueued.
		/// </summary>
		AutoResetEvent WaitHandle { get; }

		/// <summary>
		/// Adds an item to the queue.
		/// </summary>
		/// <param name="item">The item to add to the queue.</param>
		void Enqueue(T item);

		/// <summary>
		/// Clears all the items from the queue.
		/// </summary>
		void Clear();

		/// <summary>
		/// Attempts to remove and return the object at the beginning of the queue.
		/// </summary>
		/// <param name="item">
		/// When this method returns, if the operation was successful, the object removed from the beginning of the queue;
		/// otherwise, the default value of <typeparamref name="T"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <c>true</c> if an element was removed and returned from the beginning of the queue successfully; otherwise, <c>false</c>.
		/// </returns>
		bool TryDequeue(out T item);
	}

}
