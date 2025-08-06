namespace BlazorSignalProxyServices.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using BlazorSignalProxyServices.Core;
using BlazorSignalProxyServices.Core.Interceptors;
using BlazorSignalProxyServices.Core.Settings;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class SignalProxyServicesClient : IServiceCollection
{
    private readonly IServiceCollection _internalServices;
    private readonly SignalProxyClientSettings _defaultClientSettings;

    public SignalProxyServicesClient(IServiceCollection internalServices, SignalProxyClientSettings? defaultClientSettings = null)
    {
        this._internalServices = internalServices;
        this._defaultClientSettings = defaultClientSettings ?? new SignalProxyClientSettings()
        {
            DefaultClientTimeout = TimeSpan.FromMinutes(5),
        };
    }

    public ServiceDescriptor this[int index] { get => this._internalServices[index]; set => this._internalServices[index] = value; }

    public int Count => this._internalServices.Count;

    public bool IsReadOnly => this._internalServices.IsReadOnly;

    public void Add(ServiceDescriptor item) => this._internalServices.Add(item);
    public void Clear() => this._internalServices.Clear();
    public bool Contains(ServiceDescriptor item) => this._internalServices.Contains(item);
    public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => this._internalServices.CopyTo(array, arrayIndex);
    public IEnumerator<ServiceDescriptor> GetEnumerator() => this._internalServices.GetEnumerator();
    public int IndexOf(ServiceDescriptor item) => this._internalServices.IndexOf(item);
    public void Insert(int index, ServiceDescriptor item) => this._internalServices[index] = item;
    public bool Remove(ServiceDescriptor item) => this._internalServices.Remove(item);
    public void RemoveAt(int index) => this._internalServices?.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => this._internalServices.GetEnumerator();

    public IServiceCollection AddSignalProxyClient<IInterface>(SignalProxyClientSettings? clientSettings = null)
        where IInterface : class
    {
        var settings = this.CalculateClientSettings(clientSettings) ?? throw new Exception("Client settings are not set");

        // Register regular interceptors to DI
        var interceptorType = typeof(IInterceptor);
        if (settings.ProxyInterceptorTypes != null)
        {
            foreach (var interceptorCurrentType in settings.ProxyInterceptorTypes)
            {
                if (interceptorCurrentType.IsAssignableTo(interceptorType))
                {
                    //this.TryAddScoped(interceptorType, interceptorCurrentType);
                    this.TryAddScoped(interceptorCurrentType);
                }
                else
                {
                    throw new Exception($"Type {interceptorCurrentType} not assignable to IInterceptor");
                }
            }
        }

        // Register async interceptors to DI
        var asyncInterceptorType = typeof(IAsyncInterceptor);
        if (settings.ProxyAsyncInterceptorTypes != null)
        {
            foreach (var asyncInterceptorCurrentType in settings.ProxyAsyncInterceptorTypes)
            {
                if (asyncInterceptorCurrentType.IsAssignableTo(asyncInterceptorType))
                {
                    //this.TryAddScoped(asyncInterceptorType, asyncInterceptorCurrentType);
                    this.TryAddScoped(asyncInterceptorCurrentType);
                } else
                {
                    throw new Exception($"Type {asyncInterceptorCurrentType} not assignable to IAsyncInterceptor");
                }
            }
        }

        // register the proxied interface using a service provider factory function
        this.TryAddScoped<IInterface>(serviceProvider =>
        {
            // Resolve regular interceptors into a list. Assume all are registered based on the throw from earlier in this function.
            var resolvedInterceptors = new List<IInterceptor>();
            if (settings.ProxyInterceptorInstances != null)
            {
                resolvedInterceptors.AddRange(settings.ProxyInterceptorInstances);
            }

            if (settings.ProxyInterceptorTypes != null)
            {
                foreach (var interceptorType in settings.ProxyInterceptorTypes)
                {
                    var resolvedInterceptor = serviceProvider.GetRequiredService(interceptorType);
                    if (resolvedInterceptor != null)
                    {
                        resolvedInterceptors.Add((IInterceptor)resolvedInterceptor);
                    }
                }
            }
            resolvedInterceptors.Add(new ReturnDefaultNoTargetInterceptor());

            // Resolve async interceptors into a list. Assume all are registered based on the throw from earlier in this function.
            var resolvedAsyncInterceptors = new List<IAsyncInterceptor>();
            if (settings.ProxyAsyncInterceptorInstances != null)
            {
                resolvedAsyncInterceptors.AddRange(settings.ProxyAsyncInterceptorInstances);
            }

            if (settings.ProxyAsyncInterceptorTypes != null)
            {
                foreach (var asyncInterceptorType in settings.ProxyAsyncInterceptorTypes)
                {
                    var resolvedAsyncInterceptor = serviceProvider.GetRequiredService(asyncInterceptorType);
                    if (resolvedAsyncInterceptor != null)
                    {
                        resolvedAsyncInterceptors.Add((IAsyncInterceptor)resolvedAsyncInterceptor);
                    }
                }
            }

            // Get an instance of the Castle Proxy Generator
            var proxyGenerator = serviceProvider.GetRequiredService<ProxyGenerator>();

            var interceptor = new ClientSignalProxyInterceptor<IInterface>(settings);
            resolvedAsyncInterceptors.Add(interceptor);

            // Create a non-targeted interface proxy with regular interceptors.
            // This is because our interface will not have a concrete implementation.
            // The interface is used by the ClientSignalProxyInterceptor as a contract, essentially.
            var nonAsyncProxy = proxyGenerator.CreateInterfaceProxyWithoutTarget<IInterface>(resolvedInterceptors?.ToArray() ?? ([]));

            // Use the non-targeted created proxy instance for the async version
            var proxiedInterfaceInstance = proxyGenerator.CreateInterfaceProxyWithTargetInterface<IInterface>(nonAsyncProxy, resolvedAsyncInterceptors.ToArray());

            return proxiedInterfaceInstance;
        });

        return this;
    }

    private SignalProxyClientSettings CalculateClientSettings(SignalProxyClientSettings? clientSettings)
    {
        var returnSettings = clientSettings ?? new SignalProxyClientSettings();

        if (clientSettings == null)
        {
            returnSettings.HubAbsoluteUrl = this._defaultClientSettings.HubAbsoluteUrl;
            returnSettings.DefaultClientTimeout = this._defaultClientSettings.DefaultClientTimeout;
            returnSettings.ProxyInterceptorInstances = this._defaultClientSettings.ProxyInterceptorInstances;
            returnSettings.ProxyInterceptorTypes = this._defaultClientSettings.ProxyInterceptorTypes;
            returnSettings.ProxyAsyncInterceptorInstances = this._defaultClientSettings.ProxyAsyncInterceptorInstances;
            returnSettings.ProxyAsyncInterceptorTypes = this._defaultClientSettings.ProxyAsyncInterceptorTypes;
        } else if (clientSettings != null)
        {
            if (string.IsNullOrEmpty(clientSettings.HubAbsoluteUrl))
            {
                clientSettings.HubAbsoluteUrl = this._defaultClientSettings.HubAbsoluteUrl;
            }

            var interceptors = new List<IInterceptor>();
            if (this._defaultClientSettings.ProxyInterceptorInstances != null)
            {
                interceptors.AddRange(this._defaultClientSettings.ProxyInterceptorInstances);
            }

            if (clientSettings.ProxyInterceptorInstances != null)
            {
                foreach (var interceptor in clientSettings.ProxyInterceptorInstances)
                {
                    if (!interceptors.Contains(interceptor))
                    {
                        interceptors.Add(interceptor);
                    }

                }
            }
            clientSettings.ProxyInterceptorInstances = interceptors;

            var interceptorTypes = new List<Type>();
            if (this._defaultClientSettings.ProxyInterceptorTypes != null)
            {
                interceptorTypes.AddRange(this._defaultClientSettings.ProxyInterceptorTypes);
            }

            if (clientSettings.ProxyInterceptorTypes != null)
            {
                foreach (var interceptor in clientSettings.ProxyInterceptorTypes)
                {
                    if (!interceptorTypes.Contains(interceptor))
                    {
                        interceptorTypes.Add(interceptor);
                    }
                    
                }
            }
            clientSettings.ProxyInterceptorTypes = interceptorTypes;

            var asyncInterceptors = new List<IAsyncInterceptor>();
            if (this._defaultClientSettings.ProxyAsyncInterceptorInstances != null)
            {
                asyncInterceptors.AddRange(this._defaultClientSettings.ProxyAsyncInterceptorInstances);
            }

            if (clientSettings.ProxyAsyncInterceptorInstances != null)
            {
                foreach (var asyncInterceptor in clientSettings.ProxyAsyncInterceptorInstances)
                {
                    if (!asyncInterceptors.Contains(asyncInterceptor))
                    {
                        asyncInterceptors.Add(asyncInterceptor);
                    }

                }
            }
            clientSettings.ProxyAsyncInterceptorInstances = asyncInterceptors;

            var asyncInterceptorTypes = new List<Type>();
            if (this._defaultClientSettings.ProxyAsyncInterceptorTypes != null)
            {
                asyncInterceptorTypes.AddRange(this._defaultClientSettings.ProxyAsyncInterceptorTypes);
            }

            if (clientSettings.ProxyAsyncInterceptorTypes != null)
            {
                foreach (var asyncInterceptor in clientSettings.ProxyAsyncInterceptorTypes)
                {
                    if (!asyncInterceptorTypes.Contains(asyncInterceptor))
                    {
                        asyncInterceptorTypes.Add(asyncInterceptor);
                    }

                }
            }
            clientSettings.ProxyAsyncInterceptorTypes = asyncInterceptorTypes;
        }

        return returnSettings;
    }
}
