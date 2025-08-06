namespace BlazorSignalProxyServices.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

public class ProxyFunctionInvokeRequestArgument
{
    public string TypeAssemblyQualified { get; set; }
    public object? Value { get; set; }
}
