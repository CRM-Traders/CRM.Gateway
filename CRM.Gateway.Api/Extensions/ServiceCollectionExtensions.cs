using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using CRM.Gateway.Api.Configuration;
using CRM.Gateway.Api.Features.Geo.LoadBalancing;
using CRM.Gateway.Api.Features.Geo.Services;
using Yarp.ReverseProxy.LoadBalancing;
using ConcurrencyLimiterOptions = CRM.Gateway.Api.Configuration.ConcurrencyLimiterOptions;

namespace CRM.Gateway.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureRateLimiting(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(options =>
        {
            var serviceProvider = services.BuildServiceProvider();
            var rateLimitOptions = serviceProvider.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

            ConfigureGlobalLimiter(options, rateLimitOptions.Global);
            ConfigureConcurrencyLimiter(options, rateLimitOptions.Concurrency);
            ConfigureRejectionHandler(options, rateLimitOptions);
        });

        return services;
    }

    public static IServiceCollection ConfigureGeoRouting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeoRoutingOptions>(configuration.GetSection(GeoRoutingOptions.SectionName));

        var cityDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data",  "GeoLite2-City.mmdb");
        var countryDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "GeoLite2-Country.mmdb");
    
        bool hasCityDb = File.Exists(cityDbPath);
        bool hasCountryDb = File.Exists(countryDbPath);
    
        if (hasCityDb)
        {
            services.AddSingleton<IGeoIP2DatabaseReader>(provider => new GeoIP2DatabaseReader(cityDbPath));
        }
    
        if (hasCountryDb)
        {
            services.AddSingleton<IGeoIP2DatabaseReader>(provider => new GeoIP2DatabaseReader(countryDbPath));
        }
    
        if (hasCityDb || hasCountryDb)
        {
            services.AddSingleton<IIPGeolocationService, MaxMindGeolocationService>();
        }
        else
        { 
            services.AddSingleton<IIPGeolocationService>(provider => 
            {
                var options = provider.GetRequiredService<IOptions<GeoRoutingOptions>>().Value;
                var logger = provider.GetRequiredService<ILogger<IIPGeolocationService>>();
            
                logger.LogWarning("GeoLite2 databases not found. Using fallback geolocation service.");
            
                return new FallbackGeolocationService(options);
            });
        }

        return services;
    }

    public static IServiceCollection ConfigureReverseProxy(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ReverseProxyOptions>(configuration.GetSection(ReverseProxyOptions.SectionName!));

        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection(ReverseProxyOptions.SectionName!))
            .AddLoadBalancingPolicies();

        return services;
    }

    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });

        return services;
    }

    private static void ConfigureGlobalLimiter(RateLimiterOptions options, GlobalLimiterOptions globalOptions)
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = globalOptions.AutoReplenishment,
                    PermitLimit = globalOptions.PermitLimit,
                    QueueLimit = globalOptions.QueueLimit,
                    Window = TimeSpan.FromMinutes(globalOptions.WindowMinutes)
                }));
    }

    private static void ConfigureConcurrencyLimiter(RateLimiterOptions options,
        ConcurrencyLimiterOptions concurrencyOptions)
    {
        options.AddConcurrencyLimiter("ConcurrencyPolicy", limiterOptions =>
        {
            limiterOptions.PermitLimit = concurrencyOptions.PermitLimit;
            limiterOptions.QueueProcessingOrder =
                Enum.Parse<QueueProcessingOrder>(concurrencyOptions.QueueProcessingOrder);
            limiterOptions.QueueLimit = concurrencyOptions.QueueLimit;
        });
    }

    private static void ConfigureRejectionHandler(RateLimiterOptions options, RateLimitingOptions rateLimitOptions)
    {
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsync(
                rateLimitOptions.RejectionMessage,
                cancellationToken);
        };
    }
}