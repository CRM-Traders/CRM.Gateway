using System.Collections.Concurrent;
using CRM.Gateway.Api.Configuration;
using CRM.Gateway.Api.Features.Geo.Models;
using CRM.Gateway.Api.Features.Geo.Services;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace CRM.Gateway.Api.Features.Geo.LoadBalancing;

public class GeoLoadBalancer(
    IIPGeolocationService geolocationService,
    IOptions<GeoRoutingOptions> geoOptions,
    ILogger<GeoLoadBalancer> logger)
    : ILoadBalancingPolicy
{
    public string Name => "GeoProximity";

    private readonly ConcurrentDictionary<string, string> _ipRegionCache = new();
    private readonly Random _random = new();

    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> destinations)
    {
        if (destinations.Count == 0)
            return null;

        if (destinations.Count == 1)
            return destinations[0];
 
        return destinations[new Random().Next(destinations.Count)];
    }

    public async ValueTask<DestinationState?> PickDestinationAsync(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> destinations)
    {
        if (destinations.Count == 0)
            return null;

        if (destinations.Count == 1)
            return destinations[0];

        // Get client IP
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Check cache first
        if (_ipRegionCache.TryGetValue(clientIp, out var cachedRegionId))
        {
            var cachedDestinations = GetDestinationsForRegion(destinations, cachedRegionId);
            if (cachedDestinations.Count > 0)
            {
                return PickRandomDestination(cachedDestinations);
            }
        }

        try
        {
            // Get client coordinates
            var clientCoordinates = await geolocationService.GetCoordinatesAsync(clientIp);
            var bestRegion = FindNearestRegion(clientCoordinates);

            if (bestRegion != null)
            {
                var bestDestinations = GetDestinationsForRegion(destinations, bestRegion.Id);
                if (bestDestinations.Count > 0)
                {
                    // Cache for future use
                    _ipRegionCache.TryAdd(clientIp, bestRegion.Id);
                    return PickRandomDestination(bestDestinations);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in geo-based load balancing for IP: {IpAddress}", clientIp);
        }

        // Fallback - use the fallback region or random
        var fallbackRegionId = geoOptions.Value.FallbackRegion;
        var fallbackDestinations = GetDestinationsForRegion(destinations, fallbackRegionId);
        
        if (fallbackDestinations.Count > 0)
        {
            return PickRandomDestination(fallbackDestinations);
        }

        // Last resort - pick random
        return PickRandomDestination(destinations);
    }

    private ServerRegion? FindNearestRegion(GeoCoordinates clientCoords)
    {
        var regions = geoOptions.Value.ServerRegions;
        if (regions.Count == 0)
        {
            return null;
        }

        // Find nearest region using Haversine formula
        ServerRegion? nearestRegion = null;
        double minDistance = double.MaxValue;

        foreach (var region in regions)
        {
            var distance = CalculateDistance(
                clientCoords.Latitude, clientCoords.Longitude,
                region.Latitude, region.Longitude);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestRegion = region;
            }
        }

        // Check if it's within the max distance
        if (minDistance <= geoOptions.Value.MaxDistanceKilometers)
        {
            return nearestRegion;
        }

        return null;
    }

    private List<DestinationState> GetDestinationsForRegion(
        IReadOnlyList<DestinationState> destinations, 
        string regionId)
    {
        var result = new List<DestinationState>();
        
        foreach (var destination in destinations)
        {
            // Check if destination ID contains the region ID
            if (destination.DestinationId.Contains(regionId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(destination);
            }
        }

        return result;
    }

    private DestinationState PickRandomDestination(IReadOnlyList<DestinationState> destinations)
    {
        return destinations[_random.Next(destinations.Count)];
    }

    private static double CalculateDistance(
        double lat1, double lon1, 
        double lat2, double lon2)
    {
        const double r = 6371; // Earth radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}