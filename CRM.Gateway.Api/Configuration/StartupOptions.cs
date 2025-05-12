namespace CRM.Gateway.Api.Configuration;

public class StartupOptions
{
    public ServiceInfo ServiceInfo { get; set; } = new();
    public SecurityOptions Security { get; set; } = new();
}

public class ServiceInfo
{
    public string Name { get; set; } = "CRM Gateway";
    public string Version { get; set; } = "1.0.0";
}

public class SecurityOptions
{
    public bool UseHsts { get; set; } = true;
    public bool UseHttpsRedirection { get; set; } = true;

    public Dictionary<string, string> Headers { get; set; } = new()
    {
        ["X-Content-Type-Options"] = "nosniff",
        ["X-Frame-Options"] = "DENY",
        ["X-XSS-Protection"] = "1; mode=block"
    };
}