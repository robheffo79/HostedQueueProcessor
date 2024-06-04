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
			QueueWorker<String> worker = new QueueWorker<String>(_options, _logger, _mockProcessor.Object, _queueProvider);

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
			QueueWorker<String> worker = new QueueWorker<String>(_options, _logger, _mockProcessor.Object, _queueProvider);

			await worker.StartAsync(CancellationToken.None);

			_queueProvider.Enqueue("test");

			await Task.Delay(100); // Allow some time for processing

			await worker.StopAsync(CancellationToken.None);

			_mockProcessor.Verify(p => p.Process("test", It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public void ProcessItemAsync_NullItem_ThrowsArgumentNullException()
		{
			QueueWorker<String> worker = new QueueWorker<String>(_options, _logger, _mockProcessor.Object, _queueProvider);

			Assert.ThrowsExceptionAsync<ArgumentNullException>(() => worker.ProcessItemAsync(null, CancellationToken.None));
		}

		[TestMethod]
		public async Task ProcessQueue_ExceptionInProcessing_LogsError()
		{
			// Arrange
			_mockProcessor.Setup(p => p.Process(It.IsAny<String>(), It.IsAny<CancellationToken>()))
						  .ThrowsAsync(new Exception("Processing error"));

			var mockLogger = new Mock<ILogger<QueueWorker<String>>>();

			QueueWorker<String> worker = new QueueWorker<String>(_options, mockLogger.Object, _mockProcessor.Object, _queueProvider);

			// Act
			await worker.StartAsync(CancellationToken.None);

			_queueProvider.Enqueue("test");

			await Task.Delay(100); // Allow some time for processing

			// Assert that the error was logged
			mockLogger.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Processing error")),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once
			);
		}
	}
}
