using CRM.Gateway.Api.Features.Geo.LoadBalancing;
using Yarp.ReverseProxy.LoadBalancing;

namespace CRM.Gateway.Api.Extensions;

public static class ReverseProxyExtensions
{
    public static IReverseProxyBuilder AddLoadBalancingPolicies(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<ILoadBalancingPolicy, GeoLoadBalancer>();

        return builder;
    }
}