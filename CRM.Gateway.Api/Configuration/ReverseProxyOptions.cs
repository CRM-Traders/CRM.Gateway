namespace CRM.Gateway.Api.Configuration;

public class ReverseProxyOptions
{
    public const string? SectionName = "ReverseProxy";

    public Dictionary<string, RouteConfig> Routes { get; set; } = new();
    public Dictionary<string, ClusterConfig> Clusters { get; set; } = new();
}

public abstract class RouteConfig
{
    public string? ClusterId { get; set; }
    public string? RateLimiterPolicy { get; set; }
    public string? CorsPolicy { get; set; }
    public RouteMatch? Match { get; set; }
    public List<Dictionary<string, string>>? Transforms { get; set; }
}

public abstract class RouteMatch
{
    public string? Path { get; set; }
}

public abstract class ClusterConfig
{
    public string? LoadBalancingPolicy { get; set; }
    public SessionAffinityConfig? SessionAffinity { get; set; }
    public HealthCheckConfig? HealthCheck { get; set; }
    public HttpClientConfig? HttpClient { get; set; }
    public Dictionary<string, DestinationConfig>? Destinations { get; set; }
}

public abstract class SessionAffinityConfig
{
    public bool Enabled { get; set; }
    public string? Policy { get; set; }
    public string? FailurePolicy { get; set; }
    public string? AffinityKeyName { get; set; }
    public CookieConfig? Cookie { get; set; }
}

public abstract class CookieConfig
{
    public string? Domain { get; set; }
    public bool HttpOnly { get; set; }
    public bool IsEssential { get; set; }
    public string? MaxAge { get; set; }
    public string? Path { get; set; }
    public string? SameSite { get; set; }
    public string? SecurePolicy { get; set; }
}

public abstract class HealthCheckConfig
{
    public ActiveHealthCheckConfig? Active { get; set; }
    public PassiveHealthCheckConfig? Passive { get; set; }
}

public abstract class ActiveHealthCheckConfig
{
    public bool Enabled { get; set; }
    public string? Interval { get; set; }
    public string? Timeout { get; set; }
    public string? Policy { get; set; }
    public string? Path { get; set; }
}

public abstract class PassiveHealthCheckConfig
{
    public bool Enabled { get; set; }
    public string? Policy { get; set; }
    public string? ReactivationPeriod { get; set; }
}

public abstract class HttpClientConfig
{
    public string? RequestHeaderEncoding { get; set; }
    public string? ResponseHeaderEncoding { get; set; }
    public int MaxConnectionsPerServer { get; set; }
    public bool EnableMultipleHttp2Connections { get; set; }
}

public abstract class DestinationConfig
{
    public string? Address { get; set; }
    public string? Health { get; set; }
}