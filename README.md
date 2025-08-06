# BlazorSignalProxyServices

A set of libraries that enable seamless proxy method calls between Blazor Server and Blazor WebAssembly applications using SignalR connections. This library provides a transparent way to invoke methods on services from the client side as if they were local calls, while the actual execution happens on the server.

## Packages

| Package | NuGet Version | Description |
|---------|---------------|-------------|
| [BlazorSignalProxyServices.Core](https://www.nuget.org/packages/BlazorSignalProxyServices.Core/) | [![NuGet](https://img.shields.io/nuget/v/BlazorSignalProxyServices.Core.svg)](https://www.nuget.org/packages/BlazorSignalProxyServices.Core/) | Core models, settings, and shared functionality |
| [BlazorSignalProxyServices.Client](https://www.nuget.org/packages/BlazorSignalProxyServices.Client/) | [![NuGet](https://img.shields.io/nuget/v/BlazorSignalProxyServices.Client.svg)](https://www.nuget.org/packages/BlazorSignalProxyServices.Client/) | Client-side proxy services for Blazor WebAssembly |
| [BlazorSignalProxyServices.Server](https://www.nuget.org/packages/BlazorSignalProxyServices.Server/) | [![NuGet](https://img.shields.io/nuget/v/BlazorSignalProxyServices.Server.svg)](https://www.nuget.org/packages/BlazorSignalProxyServices.Server/) | Server-side SignalR hub and services |

## Features

- **Transparent Method Proxying**: Call server methods from client as if they were local
- **Real-time Communication**: Built on top of SignalR for bi-directional communication
- **Async Support**: Full support for asynchronous method calls with configurable timeouts
- **Type-Safe**: Strongly typed interfaces ensure compile-time safety
- **Configurable**: Settings for both client and server components
- **Modular**: Separate packages for Core, Client, and Server functionality

## Quick Start

### Server Setup (Blazor Server / ASP.NET Core)

1. Install the server package:
```bash
dotnet add package BlazorSignalProxyServices.Server
```

2. Configure services in `Program.cs`:
```csharp
using BlazorSignalProxyServices.Server;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR Proxy services
builder.Services.AddSignalProxyServer();

var app = builder.Build();

// Configure SignalR Proxy
app.UseSignalProxyServer();

app.Run();
```

### Client Setup (Blazor WebAssembly)

1. Install the client package:
```bash
dotnet add package BlazorSignalProxyServices.Client
```

2. Configure services in `Program.cs`:
```csharp
using BlazorSignalProxyServices.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add SignalR Proxy client services
var proxyClient = builder.Services.AddSignalProxyClient();

// Register your service interfaces for proxying
proxyClient.AddProxyService<IMyService>();

await builder.Build().RunAsync();
```

3. Use proxied services in components:
```csharp
@inject IMyService MyService

@code {
    private async Task CallServerMethod()
    {
        // This call will be proxied to the server via SignalR
        var result = await MyService.GetDataAsync();
    }
}
```

## Architecture

```
┌─────────────────────┐    SignalR     ┌─────────────────────┐
│ Blazor WebAssembly  │◄──────────────►│   Blazor Server     │
│                     │                │                     │
│ ┌─────────────────┐ │                │ ┌─────────────────┐ │
│ │ Proxy Client    │ │   Method Call  │ │ SignalR Hub     │ │
│ │ (Dynamic Proxy) │ ├────────────────┤ │ (Dispatcher)    │ │
│ └─────────────────┘ │                │ └─────────────────┘ │
│                     │                │          │          │
└─────────────────────┘                │          ▼          │
                                       │ ┌─────────────────┐ │
                                       │ │ Actual Service  │ │
                                       │ │ Implementation  │ │
                                       │ └─────────────────┘ │
                                       └─────────────────────┘
```

## How It Works

1. **Service Registration**: Register your service interfaces with the proxy client
2. **Dynamic Proxy Creation**: The library creates dynamic proxies for your service interfaces using Castle.DynamicProxy
3. **Method Interception**: When you call a method on the proxy, it's intercepted and converted into a `ProxyFunctionInvokeRequest`
4. **SignalR Communication**: The request is sent to the server via SignalR connection
5. **Server Execution**: The server hub receives the request and invokes the actual service method
6. **Response Handling**: The result is sent back to the client and returned from the proxy method

## Configuration

### Client Settings

```csharp
var clientSettings = new SignalProxyClientSettings
{
    DefaultAsyncTimeout = TimeSpan.FromSeconds(30),
    HubUrl = "/signalproxyhub"
};

builder.Services.AddSignalProxyClient(clientSettings);
```

### Server Settings

```csharp
var serverSettings = new SignalProxyServerSettings
{
    UseResponseCompression = true,
    HubPath = "/signalproxyhub"
};

builder.Services.AddSignalProxyServer(serverSettings);
```

## Advanced Usage

### Custom Interceptors

You can create custom interceptors by implementing the appropriate interfaces:

```csharp
public class CustomInterceptor : IAsyncInterceptor
{
    public void InterceptSynchronous(IInvocation invocation)
    {
        // Custom synchronous interception logic
    }

    public void InterceptAsynchronous(IInvocation invocation)
    {
        // Custom asynchronous interception logic
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        // Custom generic asynchronous interception logic
    }
}
```

### Error Handling

The library includes built-in error handling and timeout management. Failed method calls will throw exceptions on the client side, maintaining the expected behavior of local method calls.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.


## Requirements

- .NET 8.0 or later
- ASP.NET Core (for server-side)
- Blazor WebAssembly (for client-side)

## Dependencies

### Core
- Castle.Core.AsyncInterceptor
- Microsoft.Extensions.DependencyInjection.Abstractions

### Client
- Microsoft.AspNetCore.Components.WebAssembly
- Microsoft.AspNetCore.SignalR.Client
- BlazorSignalProxyServices.Core

### Server
- Microsoft.AspNetCore.Components.WebAssembly.Server
- BlazorSignalProxyServices.Core

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Changelog

### Version 1.0.0
- Initial release
- Core proxy functionality
- Client and server packages
- SignalR-based communication
- Async method support
- Configurable timeouts
