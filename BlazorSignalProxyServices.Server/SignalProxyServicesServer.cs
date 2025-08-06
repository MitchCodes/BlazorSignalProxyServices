/*
 * todo: delete?
 * 
namespace BlazorSignalProxyServices.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using BlazorSignalProxyServices.Core;
using BlazorSignalProxyServices.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

public class SignalProxyServicesServer : IServiceCollection
{
    private readonly IServiceCollection _internalServices;
    private readonly SignalProxyServerSettings? _serverSettings;

    public SignalProxyServicesServer(IServiceCollection internalServices, SignalProxyServerSettings? serverSettings = null)
    {
        this._internalServices = internalServices;
        this._serverSettings = serverSettings;
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
    

    public ISignalProxyServices AddScopedProxy<IInterface>(SignalProxyServerSettings? serverSettings = null)
        where IInterface : class
    {
        this.AddScoped<IInterface>();

        return this;
    }

    public ISignalProxyServices AddScopedProxy<IInterface, IImplemetation>(SignalProxyServerSettings? serverSettings = null)
        where IInterface : class
        where IImplemetation : class, IInterface
    {
        this.AddScoped<IInterface, IImplemetation>();

        return this;
    }

    public ISignalProxyServices AddTransientProxy<IInterface>( SignalProxyServerSettings? serverSettings = null)
        where IInterface : class
    {
        this.AddTransient<IInterface>();

        return this;
    }

    public ISignalProxyServices AddTransientProxy<IInterface, IImplemetation>( SignalProxyServerSettings? serverSettings = null)
        where IInterface : class
        where IImplemetation : class, IInterface
    {
        this.AddTransient<IInterface, IImplemetation>();

        return this;
    }

    public ISignalProxyServices AddSingletonProxy<IInterface>(IInterface instance, SignalProxyClientSettings? clientSettings = null, SignalProxyServerSettings? serverSettings = null)
        where IInterface : class
    {
        this.AddSingleton<IInterface>(instance);

        return this;
    }
}
*/
