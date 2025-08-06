namespace BlazorSignalProxyServices.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ProxyFunctionInvokeRequest
{
    public string RequestGuid { get; set; } = Guid.NewGuid().ToString();
    public string TypeFullName { get; set; }
    public string MethodName { get; set; }
    public TimeSpan? AsyncTimeout { get; set; }
    public List<ProxyFunctionInvokeRequestArgument> Args { get; set; } = new List<ProxyFunctionInvokeRequestArgument>();
}
