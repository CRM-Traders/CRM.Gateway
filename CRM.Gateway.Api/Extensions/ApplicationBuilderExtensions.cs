using Microsoft.Extensions.Options;
using CRM.Gateway.Api.Configuration;
using CRM.Gateway.Api.Features.Security.Middleware;

namespace CRM.Gateway.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder ConfigureMiddleware(this WebApplication app)
    {
        var startupOptions = app.Services.GetRequiredService<IOptions<StartupOptions>>().Value;

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
        }

        app.UseSecurityHeaders();

        if (startupOptions.Security.UseHsts)
            app.UseHsts();

        if (startupOptions.Security.UseHttpsRedirection)
            app.UseHttpsRedirection();

        app.UseRateLimiter();

        return app;
    }

    public static IApplicationBuilder ConfigureEndpoints(this WebApplication app)
    {
        var serviceInfo = app.Services.GetRequiredService<IOptions<StartupOptions>>().Value.ServiceInfo;

        app.MapHealthChecks("/health");
        app.MapGet("/", () => Results.Ok(new
        {
            service = serviceInfo.Name,
            version = serviceInfo.Version
        }));
        app.MapReverseProxy();

        return app;
    }
}