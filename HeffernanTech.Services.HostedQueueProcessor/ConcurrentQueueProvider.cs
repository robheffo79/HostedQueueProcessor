using System.Collections.Concurrent;

namespace HeffernanTech.Services.HostedQueueProcessor
{
	/// <summary>
	/// A thread-safe queue provider that uses <see cref="ConcurrentQueue{T}"/> to manage a queue of items.
	/// </summary>
	/// <typeparam name="T">The type of elements in the queue.</typeparam>
	public class ConcurrentQueueProvider<T> : IQueueProvider<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrentQueueProvider{T}"/> class.
		/// </summary>
		public ConcurrentQueueProvider()
		{
			_queue = new ConcurrentQueue<T>();
			_waitHandle = new AutoResetEvent(false);
		}

		private readonly AutoResetEvent _waitHandle;
		/// <summary>
		/// Gets the <see cref="AutoResetEvent"/> that is used to signal when an item is enqueued.
		/// </summary>
		public AutoResetEvent WaitHandle => _waitHandle;

		private readonly ConcurrentQueue<T> _queue;
		/// <summary>
		/// Clears all the items from the queue.
		/// </summary>
		public void Clear() => _queue.Clear();

		/// <summary>
		/// Adds an item to the queue.
		/// </summary>
		/// <param name="item">The item to add to the queue.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="item"/> is null.</exception>
		public void Enqueue(T item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			_queue.Enqueue(item);
			_waitHandle.Set();
		}

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
		public bool TryDequeue(out T item) => _queue.TryDequeue(out item);
	}

}
