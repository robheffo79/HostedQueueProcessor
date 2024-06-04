using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
