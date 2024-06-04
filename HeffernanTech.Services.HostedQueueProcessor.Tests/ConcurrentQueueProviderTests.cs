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

namespace HeffernanTech.Services.HostedQueueProcessor.Tests
{
	[TestClass]
	public class ConcurrentQueueProviderTests
	{
		[TestMethod]
		public void Enqueue_NullItem_ThrowsArgumentNullException()
		{
			ConcurrentQueueProvider<String> queueProvider = new ConcurrentQueueProvider<String>();

			Assert.ThrowsException<ArgumentNullException>(() => queueProvider.Enqueue(null));
		}

		[TestMethod]
		public void Enqueue_Item_SetsWaitHandle()
		{
			ConcurrentQueueProvider<String> queueProvider = new ConcurrentQueueProvider<String>();

			queueProvider.Enqueue("test");

			Assert.IsTrue(queueProvider.WaitHandle.WaitOne(0));
		}

		[TestMethod]
		public void TryDequeue_Item_ReturnsTrue()
		{
			ConcurrentQueueProvider<String> queueProvider = new ConcurrentQueueProvider<String>();
			queueProvider.Enqueue("test");

			Boolean result = queueProvider.TryDequeue(out String item);

			Assert.IsTrue(result);
			Assert.AreEqual("test", item);
		}

		[TestMethod]
		public void TryDequeue_EmptyQueue_ReturnsFalse()
		{
			ConcurrentQueueProvider<String> queueProvider = new ConcurrentQueueProvider<String>();

			Boolean result = queueProvider.TryDequeue(out String item);

			Assert.IsFalse(result);
			Assert.IsNull(item);
		}

		[TestMethod]
		public void Clear_QueueIsEmpty()
		{
			ConcurrentQueueProvider<String> queueProvider = new ConcurrentQueueProvider<String>();
			queueProvider.Enqueue("test");
			queueProvider.Clear();

			Boolean result = queueProvider.TryDequeue(out String item);

			Assert.IsFalse(result);
			Assert.IsNull(item);
		}
	}
}