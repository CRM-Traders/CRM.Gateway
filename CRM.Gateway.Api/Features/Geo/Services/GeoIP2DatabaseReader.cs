using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;

namespace CRM.Gateway.Api.Features.Geo.Services;
public interface IGeoIP2DatabaseReader
{
    Task<CountryResponse> CountryAsync(string ipAddress);
    Task<CityResponse> CityAsync(string ipAddress);
}
public class GeoIP2DatabaseReader(string dbPath) : IGeoIP2DatabaseReader, IDisposable
{
    private readonly DatabaseReader _reader = new(dbPath);

    public Task<CountryResponse> CountryAsync(string ipAddress)
    {
        return Task.FromResult(_reader.Country(ipAddress));
    }

    public Task<CityResponse> CityAsync(string ipAddress)
    {
        return Task.FromResult(_reader.City(ipAddress));
    }

    public void Dispose()
    {
        _reader?.Dispose();
    }
}