using System.Net;
using CRM.Gateway.Api.Configuration;
using CRM.Gateway.Api.Features.Geo.Models;
using Microsoft.Extensions.Options;

namespace CRM.Gateway.Api.Features.Geo.Services;

public interface IIPGeolocationService
{
    Task<string> GetCountryCodeAsync(string ipAddress);
    Task<GeoCoordinates> GetCoordinatesAsync(string ipAddress);
}

public class MaxMindGeolocationService : IIPGeolocationService
{
    private readonly IGeoIP2DatabaseReader _cityReader;
    private readonly IGeoIP2DatabaseReader _countryReader;
    private readonly ILogger<MaxMindGeolocationService> _logger;
    private readonly GeoRoutingOptions _options;

    public MaxMindGeolocationService(
        IOptions<GeoRoutingOptions> options,
        ILogger<MaxMindGeolocationService> logger,
        IEnumerable<IGeoIP2DatabaseReader> readers)
    {
        _options = options.Value;
        _logger = logger;
        
        _cityReader = readers.FirstOrDefault(r => r.GetType().Name.Contains("City"));
        _countryReader = readers.FirstOrDefault(r => r.GetType().Name.Contains("Country"));
    }

    public async Task<string> GetCountryCodeAsync(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1")
        {
            return _options.DefaultCountryCode;
        }

        try
        {
            // Try City database first as it also contains country info
            if (_cityReader != null)
            {
                try
                {
                    var response = await _cityReader.CountryAsync(ipAddress);
                    if (!string.IsNullOrEmpty(response.Country.IsoCode))
                    {
                        return response.Country.IsoCode;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to find country code for IP {IP} in City database", ipAddress);
                }
            }

            // Fall back to Country database if City doesn't work
            if (_countryReader != null)
            {
                try
                {
                    var response = await _countryReader.CountryAsync(ipAddress);
                    if (!string.IsNullOrEmpty(response.Country.IsoCode))
                    {
                        return response.Country.IsoCode;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to find country code for IP {IP} in Country database", ipAddress);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting country code for IP {IP}", ipAddress);
        }

        return _options.DefaultCountryCode;
    }

    public async Task<GeoCoordinates> GetCoordinatesAsync(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1")
        {
            return new GeoCoordinates 
            { 
                Latitude = _options.DefaultLatitude, 
                Longitude = _options.DefaultLongitude 
            };
        }

        try
        {
            if (_cityReader != null)
            {
                try
                {
                    var response = await _cityReader.CityAsync(ipAddress);
                    
                    if (response.Location.Latitude.HasValue && response.Location.Longitude.HasValue)
                    {
                        return new GeoCoordinates
                        {
                            Latitude = response.Location.Latitude.Value,
                            Longitude = response.Location.Longitude.Value
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to find coordinates for IP {IP} in database", ipAddress);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting coordinates for IP {IP}", ipAddress);
        }
 
        return new GeoCoordinates 
        { 
            Latitude = _options.DefaultLatitude, 
            Longitude = _options.DefaultLongitude 
        };
    }
}