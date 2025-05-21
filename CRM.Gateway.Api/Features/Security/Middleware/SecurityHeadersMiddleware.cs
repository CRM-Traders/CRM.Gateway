using CRM.Gateway.Api.Configuration;
using Microsoft.Extensions.Options;

namespace CRM.Gateway.Api.Features.Security.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next, IOptions<StartupOptions> startupOptions)
{
    private readonly SecurityOptions _securityOptions = startupOptions.Value.Security;

    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var header in _securityOptions.Headers)
        {
            context.Response.Headers.Append(header.Key, header.Value);
        }

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}