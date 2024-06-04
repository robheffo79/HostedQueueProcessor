// MIT License
//
// Copyright (c) 2024 Robert Heffernan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HeffernanTech.Services.HostedQueueProcessor
{
	/// <summary>
	/// A background worker that processes items from a queue using a specified processor.
	/// </summary>
	/// <typeparam name="T">The type of elements in the queue.</typeparam>
	public class QueueWorker<T> : IHostedService
	{
		private readonly List<Task> _runningTasks;
		private readonly AutoResetEvent _runningTaskWaitHandle;
		private readonly QueueWorkerOptions _options;

		private readonly IQueueProcessor<T> _queueProcessor;
		private readonly IQueueProvider<T> _queueProvider;
		private readonly ILogger<QueueWorker<T>> _logger;

		private CancellationTokenSource _queueProcessorCancellationToken;
		private Task _queueProcessorTask;

		/// <summary>
		/// Initializes a new instance of the <see cref="QueueWorker{T}"/> class.
		/// </summary>
		/// <param name="options">The options for configuring the queue worker.</param>
		/// <param name="logger">The logger used to log information and errors.</param>
		/// <param name="queueProcessor">The processor used to process queue items.</param>
		/// <param name="queueProvider">The provider that supplies the queue of items.</param>
		public QueueWorker(IOptions<QueueWorkerOptions> options, ILogger<QueueWorker<T>> logger, IQueueProcessor<T> queueProcessor, IQueueProvider<T> queueProvider)
		{
			_queueProcessor = queueProcessor;
			_queueProvider = queueProvider;
			_logger = logger;

			_runningTasks = new List<Task>();
			_runningTaskWaitHandle = new AutoResetEvent(false);
			_options = options.Value;
		}

		/// <summary>
		/// Starts the background queue worker.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous start operation.</returns>
		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting queue worker.");

			_queueProcessorCancellationToken = new CancellationTokenSource();
			_queueProcessorTask = Task.Factory.StartNew(() => ProcessQueue(_queueProcessorCancellationToken.Token), _queueProcessorCancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			_logger.LogInformation("Started queue worker.");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the background queue worker.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous stop operation.</returns>
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping queue worker.");

			_queueProcessorCancellationToken.Cancel();
			await _queueProcessorTask.ConfigureAwait(false);

			_logger.LogInformation("Stopped queue worker.");
		}

		/// <summary>
		/// Processes the queue, dequeuing items and processing them using the specified processor.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		private void ProcessQueue(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					// Wait until one of the conditions: WaitHandle is signaled, cancellation token is cancelled, or a sub-task completes
					WaitHandle.WaitAny(new WaitHandle[] { _queueProvider.WaitHandle, cancellationToken.WaitHandle, _runningTaskWaitHandle });

					// Clean up completed tasks
					_runningTasks.RemoveAll(t => t.IsCompleted);

					// Dequeue and process items until the MAX_WORKER_TASKS limit is reached
					while (_runningTasks.Count < _options.MaxWorkerTasks && _queueProvider.TryDequeue(out T item))
					{
						Task task = ProcessItemAsync(item, cancellationToken);
						_runningTasks.Add(task);
					}
				}
				catch (OperationCanceledException)
				{
					// Handle cancellation
					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred while processing queue.");
				}
			}

			// Cancel all running tasks and wait for them to complete
			Task.WhenAll(_runningTasks).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Processes a single queue item using the specified processor.
		/// </summary>
		/// <param name="item">The item to process.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous processing operation.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="item"/> is null.</exception>
		internal async Task ProcessItemAsync(T item, CancellationToken cancellationToken)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			try
			{
				await _queueProcessor.Process(item, cancellationToken)
									 .ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"An error occurred while processing item: {ex.Message}");
			}
			finally
			{
				_runningTaskWaitHandle.Set();
			}
		}
	}
}
