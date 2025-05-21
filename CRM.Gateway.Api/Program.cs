using CRM.Gateway.Api.Configuration;
using CRM.Gateway.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StartupOptions>(builder.Configuration.GetSection("Startup"));
builder.Services.ConfigureRateLimiting(builder.Configuration);
builder.Services.ConfigureGeoRouting(builder.Configuration);  
builder.Services.ConfigureReverseProxy(builder.Configuration);
builder.Services.ConfigureHealthChecks();
builder.Services.ConfigureLogging();

var app = builder.Build();

app.ConfigureMiddleware();
app.ConfigureEndpoints();

app.Run();