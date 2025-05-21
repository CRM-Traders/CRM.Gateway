namespace CRM.Gateway.Api.Configuration;

public class GeoRoutingOptions
{
    public const string SectionName = "GeoRouting";

    public bool Enabled { get; set; } = true;
    public string DefaultCountryCode { get; set; } = "GE";
    public double DefaultLatitude { get; set; } = 41.7151;
    public double DefaultLongitude { get; set; } = 44.8271;
    public string FallbackRegion { get; set; } = "eu";
    public int MaxDistanceKilometers { get; set; } = 5000;
    public int CacheDurationHours { get; set; } = 24;
    public List<ServerRegion> ServerRegions { get; set; } = new();
}

public class ServerRegion
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<string> PreferredCountries { get; set; } = new();
}
