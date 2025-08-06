namespace BlazorSignalProxyServices.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorSignalProxyServices.Core;
using Microsoft.AspNetCore.SignalR;


public class SignalProxyServerHub : Hub
{
    public async Task<ProxyFunctionInvokeResponse> InvokeProxyFunction(ProxyFunctionInvokeRequest request)
    {
        var response = new ProxyFunctionInvokeResponse { RequestGuid = request.RequestGuid };

        try
        {
            // Get the interface type from the request
            var type = Type.GetType(request.TypeFullName) ?? throw new InvalidOperationException($"Type {request.TypeFullName} not found.");

            // Get the method information using reflection
            var method = type.GetMethod(request.MethodName) ?? throw new InvalidOperationException($"Method {request.MethodName} not found in type {request.TypeFullName}.");

            // Resolve the service from the DI container
            var service = this.Context.GetHttpContext().RequestServices.GetService(type) ?? throw new InvalidOperationException($"Service of type {request.TypeFullName} not found.");

            // Prepare CancellationTokenSource based on AsyncTimeout or default
            using var cts = new CancellationTokenSource(request.AsyncTimeout ?? TimeSpan.FromMinutes(30));
            var parameters = method.GetParameters();
            var argsWithCancellationToken = parameters.Select((p, index) =>
            {
                // Replace any cancellation tokens/sources with the new one
                if (p.ParameterType == typeof(CancellationTokenSource))
                {
                    return cts;
                } else if (p.ParameterType == typeof(CancellationToken))
                {
                    return cts.Token;
                }

                if (request.Args.Count > index)
                {
                    var indexValue = request.Args[index];

                    if (indexValue.Value is System.Text.Json.JsonElement jsonElement)
                    {
                        var convertedValue = this.ConvertJsonElementToObject(jsonElement, indexValue.TypeAssemblyQualified);
                        return convertedValue;
                    } else
                    {
                        return request.Args[index];
                    }
                }

                // In-case of any default parameters
                return p.DefaultValue;
            }).ToArray();

            var result = method.Invoke(service, argsWithCancellationToken);

            if (result is Task task)
            {
                if (await Task.WhenAny(task, Task.Delay(request.AsyncTimeout ?? TimeSpan.FromMinutes(30), cts.Token)) == task)
                {
                    await task; // Ensure any exceptions are rethrown
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }

                var taskResultProperty = task.GetType().GetProperty("Result");
                response.Result = taskResultProperty?.GetValue(task);
            }
            else
            {
                response.Result = result;
            }
        }
        catch (Exception ex)
        {
            // Capture and return the exception details
            response.Exception = this.ConvertExceptionToResponseException(ex.InnerException ?? ex);
        }

        return response;
    }

    private object? ConvertJsonElementToObject(JsonElement element, string typeName)
    {
        var json = element.GetRawText();
        var type = Type.GetType(typeName) ?? throw new Exception("Type could not be parsed");
        return JsonSerializer.Deserialize(json, type);
    }

    private ProxyFunctionInvokeResponseException ConvertExceptionToResponseException(Exception exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var responseException = new ProxyFunctionInvokeResponseException
        {
            ExceptionFullyQualifiedType = exception.GetType().AssemblyQualifiedName,
            ExceptionMessage = exception.Message
        };

        if (exception.InnerException != null)
        {
            responseException.InnerException = this.ConvertExceptionToResponseException(exception.InnerException);
        }

        return responseException;
    }
}
