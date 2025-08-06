namespace BlazorSignalProxyServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorSignalProxyServices.Core.Settings;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class SignalProxyServicesClientExtensions
{
    public static SignalProxyServicesClient AddSignalProxyClient(this IServiceCollection services, SignalProxyClientSettings? clientSettings = null)
    {
        clientSettings ??= new SignalProxyClientSettings();

        var proxyServices = new SignalProxyServicesClient(services, clientSettings);

        services.TryAddSingleton(new ProxyGenerator());

        return proxyServices;
    }

    /*
    public static WebApplication UseSignalProxyClient(this WebApplication app, SignalProxyClientSettings? clientSettings = null)
    {
        clientSettings ??= new SignalProxyClientSettings();

        return app;
    }
    */
}
