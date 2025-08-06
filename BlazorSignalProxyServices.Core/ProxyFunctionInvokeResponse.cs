namespace BlazorSignalProxyServices.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ProxyFunctionInvokeResponseException
{
    public string? ExceptionFullyQualifiedType { get; set; }
    public string? ExceptionMessage { get; set; }
    public ProxyFunctionInvokeResponseException? InnerException { get; set; }
}

public class ProxyFunctionInvokeResponse
{
    public string RequestGuid { get; set; }
    public ProxyFunctionInvokeResponseException? Exception { get; set; }
    public object? Result { get; set; }
}
