# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build
```bash
dotnet build
```

### Clean and Rebuild
```bash
dotnet clean
dotnet build
```

### Create NuGet Packages
```bash
dotnet pack
```
Packages are automatically generated on build due to `<GeneratePackageOnBuild>true</GeneratePackageOnBuild>` in project files.

### Run Individual Project Tests
No test projects are currently present in this solution.

## Architecture Overview

This is a .NET 8.0 solution providing SignalR-based proxy services for Blazor applications. The solution enables seamless method calls between Blazor Server and Blazor WebAssembly clients using SignalR connections.

### Project Structure

- **BlazorSignalProxyServices.Core**: Shared models, settings, and core functionality
  - Contains request/response models (`ProxyFunctionInvokeRequest`, `ProxyFunctionInvokeResponse`)
  - Configuration settings (`SignalProxyClientSettings`, `SignalProxyServerSettings`)
  - Base interceptors (`ReturnDefaultNoTargetInterceptor`)

- **BlazorSignalProxyServices.Client**: Client-side proxy generation for Blazor WebAssembly
  - Uses Castle.DynamicProxy to create interface proxies
  - `SignalProxyServicesClient` handles proxy registration and configuration
  - `ClientSignalProxyInterceptor` intercepts method calls and routes them via SignalR

- **BlazorSignalProxyServices.Server**: Server-side SignalR hub and service resolution
  - `SignalProxyServerHub` receives proxy requests and invokes actual service methods
  - Uses reflection to resolve services from DI and invoke methods
  - Handles async operations with configurable timeouts

### Key Technologies

- **Castle.DynamicProxy**: Creates runtime proxies for interface method interception
- **SignalR**: Provides real-time communication between client and server
- **Dependency Injection**: Services are resolved from the DI container on the server side
- **Reflection**: Method calls are dynamically invoked on the server using reflection

### Proxy Flow

1. Client registers service interfaces using `AddSignalProxyClient<IInterface>()`
2. Dynamic proxies are created using Castle.DynamicProxy
3. Method calls are intercepted by `ClientSignalProxyInterceptor`
4. Requests are serialized and sent via SignalR to `SignalProxyServerHub`
5. Server resolves the actual service from DI and invokes the method
6. Results are serialized and returned to the client proxy

### Configuration

Both client and server have configurable settings:
- **Client**: Hub URL, timeouts, custom interceptors
- **Server**: Hub path, response compression, custom interceptors

Services must be registered in the DI container on the server side to be available for proxy calls.