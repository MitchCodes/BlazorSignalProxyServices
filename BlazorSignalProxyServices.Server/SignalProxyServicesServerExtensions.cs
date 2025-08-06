namespace BlazorSignalProxyServices.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorSignalProxyServices.Core;
using BlazorSignalProxyServices.Core.Settings;
using BlazorSignalProxyServices.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

public static class SignalProxyServicesServerExtensions
{
    public static IServiceCollection AddSignalProxyServer(this IServiceCollection services, SignalProxyServerSettings? serverSettings = null)
    {
        serverSettings ??= new SignalProxyServerSettings();

        services.AddSignalR( o =>
        {
            o.EnableDetailedErrors = true;
        });

        if (serverSettings.UseResponseCompression)
        {
            services.AddResponseCompression(opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                      new[] { "application/octet-stream" }));
        }

        return services;
    }

    public static WebApplication UseSignalProxyServer(this WebApplication app, SignalProxyServerSettings? serverSettings = null)
    {
        serverSettings ??= new SignalProxyServerSettings();

        if (serverSettings.UseResponseCompression)
        {
            app.UseResponseCompression();
        }

        app.MapHub<SignalProxyServerHub>(serverSettings.EndpointRelative);

        return app;
    }
}
