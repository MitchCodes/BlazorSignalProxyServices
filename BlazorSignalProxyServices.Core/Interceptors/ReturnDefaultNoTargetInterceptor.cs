namespace BlazorSignalProxyServices.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

public class ReturnDefaultNoTargetInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation) => invocation.ReturnValue = this.GetDefault(invocation.Method.ReturnType);

    private object? GetDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }
}
