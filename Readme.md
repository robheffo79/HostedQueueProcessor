# HeffernanTech.Services.HostedQueueProcessor

## Overview

The `HeffernanTech.Services.HostedQueueProcessor` package provides a hosted service for processing items in a queue using a specified processor. This package is designed to be used with dependency injection in an ASP.NET Core application.

## Features

- **Thread-Safe Queue Management**: Uses a concurrent queue to manage items safely across multiple threads.
- **Background Processing**: Processes queue items in the background using a hosted service.
- **Configurable Concurrency**: Allows configuration of the maximum number of concurrent worker tasks.
- **Integration with Dependency Injection**: Easily integrates with the ASP.NET Core dependency injection system.

## Installation

To install the `HeffernanTech.Services.HostedQueueProcessor` package, use the following command in your .NET project:

```sh
dotnet add package HeffernanTech.Services.HostedQueueProcessor
```

## Usage

### Setup

1. **Define a Queue Processor**: Implement the `IQueueProcessor<T>` interface to define how to process each item in the queue.

    ```csharp
    public class MyQueueProcessor : IQueueProcessor<MyItem>
    {
        public async Task Process(MyItem item, CancellationToken cancellationToken)
        {
            // Your processing logic here
        }
    }
    ```

2. **Configure Services**: In your `Startup.cs` or wherever you configure services, add the hosted queue worker to the service collection.

    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IQueueProcessor<MyItem>, MyQueueProcessor>();
        services.AddSingleton<IQueueProvider<MyItem>, ConcurrentQueueProvider<MyItem>>();

        services.AddQueueWorker<MyItem>(options =>
        {
            options.MaxWorkerTasks = 4; // Set the maximum number of concurrent worker tasks
        });
    }
    ```

### Example

Below is a complete example of setting up and using the hosted queue processor in an ASP.NET Core application.

1. **Implement the Queue Processor**:

    ```csharp
    public class MyQueueProcessor : IQueueProcessor<MyItem>
    {
        public async Task Process(MyItem item, CancellationToken cancellationToken)
        {
            // Simulate some work
            await Task.Delay(1000, cancellationToken);
            Console.WriteLine($"Processed item: {item.Name}");
        }
    }

    public class MyItem
    {
        public string Name { get; set; }
    }
    ```

2. **Configure the Services**:

    ```csharp
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IQueueProcessor<MyItem>, MyQueueProcessor>();
            services.AddSingleton<IQueueProvider<MyItem>, ConcurrentQueueProvider<MyItem>>();

            services.AddHostedQueueWorker<MyItem>(options =>
            {
                options.MaxWorkerTasks = 4; // Configure the maximum number of worker tasks
            });

            // Other service configurations
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Application configuration
        }
    }
    ```

3. **Enqueue Items**: Enqueue items to be processed by the queue worker.

    ```csharp
    public class MyController : ControllerBase
    {
        private readonly IQueueProvider<MyItem> _queueProvider;

        public MyController(IQueueProvider<MyItem> queueProvider)
        {
            _queueProvider = queueProvider;
        }

        [HttpPost("enqueue")]
        public IActionResult Enqueue([FromBody] MyItem item)
        {
            _queueProvider.Enqueue(item);
            return Ok("Item enqueued");
        }
    }
    ```

### Running the Application

Start your ASP.NET Core application. The `QueueWorker` will run in the background, processing items from the queue using the defined `MyQueueProcessor`.

## Options

### `QueueWorkerOptions`

- **MaxWorkerTasks**: The maximum number of worker tasks that can run concurrently. The default value is the number of processors available on the machine.

```csharp
public class QueueWorkerOptions
{
    public int MaxWorkerTasks { get; set; } = Environment.ProcessorCount;
}
```

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

---

For more information, please refer to the official documentation and API reference.

---

Happy coding!