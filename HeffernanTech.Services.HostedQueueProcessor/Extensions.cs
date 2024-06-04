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

using Microsoft.Extensions.DependencyInjection;

namespace HeffernanTech.Services.HostedQueueProcessor
{
	/// <summary>
	/// Extension methods for adding a queue worker to an <see cref="IServiceCollection"/>.
	/// </summary>
	public static class HostedQueueProcessorExtensions
	{
		/// <summary>
		/// Adds a <see cref="QueueWorker{T}"/> service to the specified <see cref="IServiceCollection"/>.
		/// </summary>
		/// <typeparam name="T">The type of elements to be processed by the queue worker.</typeparam>
		/// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
		/// <param name="options">An optional action to configure the <see cref="QueueWorkerOptions"/>.</param>
		/// <returns>The same service collection so that multiple calls can be chained.</returns>
		public static IServiceCollection AddQueueWorker<T>(this IServiceCollection services, Action<QueueWorkerOptions> options = null)
		{
			services.AddOptions<QueueWorkerOptions>().Configure(configure =>
			{
				options?.Invoke(configure);
			});

			services.AddHostedService<QueueWorker<T>>();

			return services;
		}
	}
}
