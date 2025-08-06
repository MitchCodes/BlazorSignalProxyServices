namespace BlazorSignalProxyServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorSignalProxyServices.Core;
using BlazorSignalProxyServices.Core.Settings;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable IDE0022 // Use expression body for method

public class ClientSignalProxyInterceptor<T> : AsyncInterceptorBase where T : class
{
    private readonly SignalProxyClientSettings _clientSettings;

    public ClientSignalProxyInterceptor(SignalProxyClientSettings? clientSettings = null) => this._clientSettings = clientSettings ?? new SignalProxyClientSettings();

    protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        if (typeof(Task).IsAssignableFrom(invocation.Method.ReturnType))
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            if (task != null)
            {
                await task;
            }

            // is async
            await this.InvokeInternalAsync(invocation.Method, invocation.Arguments).ConfigureAwait(false);
        } else
        {
            throw new Exception("Cannot use Signal Proxy with syncronous functions");
        }
    }

    protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        if (typeof(Task).IsAssignableFrom(invocation.Method.ReturnType))
        {
            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            if (task != null)
            {
                await task;
            }

            var invokeResponse = await this.InvokeInternalAsync(invocation.Method, invocation.Arguments).ConfigureAwait(false);

            if (invokeResponse.Result is JsonDocument jsonDocument)
            {
                return JsonSerializer.Deserialize<TResult>(jsonDocument);
            }
            else if (invokeResponse.Result is JsonElement jsonElement)
            {
                return (TResult)JsonSerializer.Deserialize(jsonElement, typeof(TResult));
            }

            return (TResult)invokeResponse.Result;
        } else
        {
            throw new Exception("Cannot use Signal Proxy with syncronous functions");
        }
    }

    private async Task<ProxyFunctionInvokeResponse> InvokeInternalAsync(MethodInfo? method, object?[]? args)
    {
        var arguments = new List<ProxyFunctionInvokeRequestArgument>();
        foreach (var arg in args)
        {
            arguments.Add(new ProxyFunctionInvokeRequestArgument()
            {
                TypeAssemblyQualified = arg.GetType().AssemblyQualifiedName ?? throw new Exception("Type is not qualified"),
                Value = arg
            });
        }

        var request = new ProxyFunctionInvokeRequest
        {
            TypeFullName = typeof(T).AssemblyQualifiedName ?? throw new Exception("Type AssemblyQualifiedName is not set"),
            MethodName = method.Name,
            Args = arguments,
            AsyncTimeout = this._clientSettings.DefaultClientTimeout
        };

        var cts = new CancellationTokenSource(this._clientSettings.DefaultClientTimeout ?? TimeSpan.FromMinutes(5));

        //Console.WriteLine("Before signalr proxy");
        HubConnection? hubConnection = null;
        try
        {
            hubConnection = new HubConnectionBuilder().WithUrl(this._clientSettings.HubAbsoluteUrl).Build();
            await hubConnection.StartAsync().ConfigureAwait(false);

            var responseTask = hubConnection.InvokeAsync<ProxyFunctionInvokeResponse>("InvokeProxyFunction", request, cts.Token);

            if (await Task.WhenAny(responseTask, Task.Delay(this._clientSettings.DefaultClientTimeout ?? TimeSpan.FromMinutes(5), cts.Token)).ConfigureAwait(false) == responseTask)
            {
                var response = await responseTask; // Ensure any exceptions are rethrown

                if (response.Exception != null)
                {
                    var exception = this.ConvertResponseExceptionToException(response.Exception);
                    throw exception;
                }

                await hubConnection?.StopAsync();
                hubConnection?.DisposeAsync();

                return response;
            }
            else
            {
                throw new TimeoutException($"The operation has timed out after {this._clientSettings.DefaultClientTimeout?.TotalSeconds} seconds.");
            }
        }
        catch (AggregateException ae)
        {
            await hubConnection?.StopAsync();
            hubConnection?.DisposeAsync();

            throw ae.Flatten().InnerException ?? ae;
        }
        catch (Exception)
        {
            await hubConnection?.StopAsync();
            hubConnection?.DisposeAsync();

            throw;
        }
    }

    private Exception ConvertResponseExceptionToException(ProxyFunctionInvokeResponseException responseException)
    {
        if (responseException == null)
        {
            throw new ArgumentNullException(nameof(responseException));
        }

        // Try to get the Type from the fully qualified type name
        var exceptionType = Type.GetType(responseException.ExceptionFullyQualifiedType ?? throw new Exception("Response exception does not have a qualified type")) ?? typeof(Exception);

        // Prepare the message for the exception. If it's null or empty, use a default message.
        var message = responseException.ExceptionMessage ?? "An error occurred.";

        // Create an instance of the exception. If the type does not have a constructor that
        // takes a single string argument (for the message), fall back to a parameterless constructor
        // and then a generic Exception if needed.
        Exception exceptionInstance;
        try
        {
            Exception? innerException = null;
            if (responseException.InnerException != null)
            {
                // If there's an inner exception, recursively convert it and set it on the current exception instance
                innerException = this.ConvertResponseExceptionToException(responseException.InnerException);
            }

            if (innerException != null)
            {
                exceptionInstance = (Exception)Activator.CreateInstance(exceptionType, message, innerException);
            } else if (responseException.ExceptionMessage != null)
            {
                exceptionInstance = (Exception)Activator.CreateInstance(exceptionType, message);
            } else
            {
                exceptionInstance = (Exception)Activator.CreateInstance(exceptionType);
            }
        }
        catch
        {
            try
            {
                // Try to use the parameterless constructor if available
                exceptionInstance = (Exception)Activator.CreateInstance(exceptionType);
            }
            catch
            {
                // If all else fails, default to using a generic Exception
                exceptionInstance = new Exception(message);
            }
        }        

        return exceptionInstance;
    }
}
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore IDE0058 // Expression value is never used
#pragma warning restore IDE0022 // Use expression body for method
