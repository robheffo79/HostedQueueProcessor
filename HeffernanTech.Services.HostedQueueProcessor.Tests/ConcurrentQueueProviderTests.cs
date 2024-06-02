namespace HeffernanTech.Services.HostedQueueProcessor.Tests
{
	[TestClass]
	public class ConcurrentQueueProviderTests
	{
		[TestMethod]
		public void Enqueue_NullItem_ThrowsArgumentNullException()
		{
			var queueProvider = new ConcurrentQueueProvider<string>();

			Assert.ThrowsException<ArgumentNullException>(() => queueProvider.Enqueue(null));
		}

		[TestMethod]
		public void Enqueue_Item_SetsWaitHandle()
		{
			var queueProvider = new ConcurrentQueueProvider<string>();

			queueProvider.Enqueue("test");

			Assert.IsTrue(queueProvider.WaitHandle.WaitOne(0));
		}

		[TestMethod]
		public void TryDequeue_Item_ReturnsTrue()
		{
			var queueProvider = new ConcurrentQueueProvider<string>();
			queueProvider.Enqueue("test");

			var result = queueProvider.TryDequeue(out var item);

			Assert.IsTrue(result);
			Assert.AreEqual("test", item);
		}

		[TestMethod]
		public void TryDequeue_EmptyQueue_ReturnsFalse()
		{
			var queueProvider = new ConcurrentQueueProvider<string>();

			var result = queueProvider.TryDequeue(out var item);

			Assert.IsFalse(result);
			Assert.IsNull(item);
		}

		[TestMethod]
		public void Clear_QueueIsEmpty()
		{
			var queueProvider = new ConcurrentQueueProvider<string>();
			queueProvider.Enqueue("test");
			queueProvider.Clear();

			var result = queueProvider.TryDequeue(out var item);

			Assert.IsFalse(result);
			Assert.IsNull(item);
		}
	}
}