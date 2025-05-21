using CRM.Gateway.Api.Configuration;
using CRM.Gateway.Api.Features.Geo.Models;

namespace CRM.Gateway.Api.Features.Geo.Services;

public class FallbackGeolocationService(GeoRoutingOptions options) : IIPGeolocationService
{
    public Task<string> GetCountryCodeAsync(string ipAddress)
    {
        return Task.FromResult(options.DefaultCountryCode);
    }

    public Task<GeoCoordinates> GetCoordinatesAsync(string ipAddress)
    {
        return Task.FromResult(new GeoCoordinates
        {
            Latitude = options.DefaultLatitude,
            Longitude = options.DefaultLongitude
        });
    }
}