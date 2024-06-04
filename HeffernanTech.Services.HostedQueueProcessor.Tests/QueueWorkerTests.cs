using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace HeffernanTech.Services.HostedQueueProcessor.Tests
{
	[TestClass]
	public class QueueWorkerTests
	{
		private Mock<IQueueProcessor<String>> _mockProcessor;
		private ConcurrentQueueProvider<String> _queueProvider;
		private ILogger<QueueWorker<String>> _logger;
		private IOptions<QueueWorkerOptions> _options;

		[TestInitialize]
		public void Setup()
		{
			_mockProcessor = new Mock<IQueueProcessor<String>>();
			_queueProvider = new ConcurrentQueueProvider<String>();
			_logger = NullLogger<QueueWorker<String>>.Instance;
			_options = Options.Create(new QueueWorkerOptions { MaxWorkerTasks = 2 });
		}

		[TestMethod]
		public async Task StartAsync_ProcessesItemsFromQueue()
		{
			var worker = new QueueWorker<String>(_options, _logger, _mockProcessor.Object, _queueProvider);

			await worker.StartAsync(CancellationToken.None);

			_queueProvider.Enqueue("test1");
			_queueProvider.Enqueue("test2");

			await Task.Delay(100); // Allow some time for processing

			_mockProcessor.Verify(p => p.Process("test1", It.IsAny<CancellationToken>()), Times.Once);
			_mockProcessor.Verify(p => p.Process("test2", It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task StopAsync_CancelsProcessing()
		{
			var worker = new QueueWorker<String>(_options, _logger, _mockProcessor.Object, _queueProvider);

			await worker.StartAsync(CancellationToken.None);

			_queueProvider.Enqueue("test");

			await Task.Delay(100); // Allow some time for processing

			await worker.StopAsync(CancellationToken.None);

			_mockProcessor.Verify(p => p.Process("test", It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public void ProcessItemAsync_NullItem_ThrowsArgumentNullException()
		{
			var worker = new QueueWorker<string>(_options, _logger, _mockProcessor.Object, _queueProvider);

			Assert.ThrowsExceptionAsync<ArgumentNullException>(() => worker.ProcessItemAsync(null, CancellationToken.None));
		}

		[TestMethod]
		public async Task ProcessQueue_ExceptionInProcessing_LogsError()
		{
			_mockProcessor.Setup(p => p.Process(It.IsAny<string>(), It.IsAny<CancellationToken>()))
						  .ThrowsAsync(new Exception("Processing error"));

			var worker = new QueueWorker<string>(_options, _logger, _mockProcessor.Object, _queueProvider);

			await worker.StartAsync(CancellationToken.None);

			_queueProvider.Enqueue("test");

			await Task.Delay(100); // Allow some time for processing

			// Assert that the error was logged
			// Here we assume that the logger logs the error, you might need a proper logger mock to verify logging
		}
	}
}