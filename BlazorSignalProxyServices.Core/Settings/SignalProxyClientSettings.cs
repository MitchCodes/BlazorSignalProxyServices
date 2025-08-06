namespace BlazorSignalProxyServices.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

public class SignalProxyClientSettings
{
    public string HubAbsoluteUrl { get; set; }
    public TimeSpan? DefaultClientTimeout { get; set; } = TimeSpan.FromMinutes(5);
    /// <summary>
    /// Syncronous Castle.Core interceptor instances that do not use Dependency Injection
    /// </summary>
    public IEnumerable<IInterceptor> ProxyInterceptorInstances { get; set; } = new List<IInterceptor>();
    /// <summary>
    /// Syncronous Castle.Core interceptors that are registered for Dependency Injection
    /// </summary>
    public IEnumerable<Type> ProxyInterceptorTypes { get; set; } = new List<Type>();
    /// <summary>
    /// Asyncronous Castle.Core interceptor instances that do not use Dependency Injection
    /// </summary>
    public IEnumerable<IAsyncInterceptor> ProxyAsyncInterceptorInstances { get; set; } = new List<IAsyncInterceptor>();
    /// <summary>
    /// Asyncronous Castle.Core interceptors that are registered for dependency injection
    /// </summary>
    public IEnumerable<Type> ProxyAsyncInterceptorTypes { get; set; } = new List<Type>();
}
