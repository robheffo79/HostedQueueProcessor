namespace HeffernanTech.Services.HostedQueueProcessor.Abstractions
{
	/// <summary>
	/// Defines a processor that handles items from a queue.
	/// </summary>
	/// <typeparam name="T">The type of elements to be processed.</typeparam>
	public interface IQueueProcessor<T>
	{
		/// <summary>
		/// Processes the specified item.
		/// </summary>
		/// <param name="item">The item to process.</param>
		/// <param name="token">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous processing operation.</returns>
		Task Process(T item, CancellationToken token);
	}

}
