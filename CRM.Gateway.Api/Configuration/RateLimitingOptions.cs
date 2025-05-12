namespace CRM.Gateway.Api.Configuration;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public GlobalLimiterOptions Global { get; set; } = new();
    public ConcurrencyLimiterOptions Concurrency { get; set; } = new();
    public string RejectionMessage { get; set; } = "Too many requests. Please retry later.";
}

public class GlobalLimiterOptions
{
    public bool AutoReplenishment { get; set; } = true;
    public int PermitLimit { get; set; } = 100;
    public int QueueLimit { get; set; } = 50;
    public double WindowMinutes { get; set; } = 1;
}

public class ConcurrencyLimiterOptions
{
    public int PermitLimit { get; set; } = 50;
    public string QueueProcessingOrder { get; set; } = "OldestFirst";
    public int QueueLimit { get; set; } = 25;
}