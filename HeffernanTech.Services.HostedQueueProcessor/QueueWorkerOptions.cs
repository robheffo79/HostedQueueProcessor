namespace HeffernanTech.Services.HostedQueueProcessor
{
	/// <summary>
	/// Options for configuring the <see cref="QueueWorker{T}"/>.
	/// </summary>
	public class QueueWorkerOptions
	{
		/// <summary>
		/// Gets or sets the maximum number of worker tasks that can run concurrently.
		/// Default value is the number of processors available on the machine.
		/// </summary>
		public int MaxWorkerTasks { get; set; } = Environment.ProcessorCount;
	}

}