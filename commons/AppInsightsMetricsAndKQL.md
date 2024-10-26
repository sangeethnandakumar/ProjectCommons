# AppInsghts Metrics and KQL
![image](https://github.com/user-attachments/assets/0fb187a2-ae67-4278-bcbf-c9a184e79a3a)

## Use Metrics Like This
```cs
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace parinaybharat.api.presentation.Middlewares
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TelemetryClient _telemetryClient;

        public RequestTimingMiddleware(RequestDelegate next, TelemetryClient telemetryClient)
        {
            _next = next;
            _telemetryClient = telemetryClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await Task.Delay(new Random().Next(0, 100));
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Get the endpoint name
                var endpoint = context.GetEndpoint();
                var endpointName = endpoint?.DisplayName ?? context.Request.Path;

                // Track the metric with more properties for better filtering
                _telemetryClient.TrackMetric("Request Duration", stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "EndpointName", endpointName },
                    { "Method", context.Request.Method },
                    { "StatusCode", context.Response.StatusCode.ToString() }
                });

                _telemetryClient.TrackMetric("DB Query Duration", stopwatch.ElapsedMilliseconds - 5, new Dictionary<string, string>
                {
                    { "EndpointName", endpointName },
                    { "Method", context.Request.Method },
                    { "StatusCode", context.Response.StatusCode.ToString() }
                });

                _telemetryClient.TrackMetric("ForgeRock Auth Duration", stopwatch.ElapsedMilliseconds - 10, new Dictionary<string, string>
                {
                    { "EndpointName", endpointName },
                    { "Method", context.Request.Method },
                    { "StatusCode", context.Response.StatusCode.ToString() }
                });
            }
        }
    }
}
```

## KQL
```kql
customMetrics 
| where name in ("Request Duration", "DB Query Duration", "ForgeRock Auth Duration")
| extend endpoint = tostring(customDimensions.EndpointName)
| extend method = tostring(customDimensions.Method)
| extend statusCode = tostring(customDimensions.StatusCode)
| extend metricType = case(
    name == "Request Duration", "1. Total Request", 
    name == "DB Query Duration", "2. DB Query",
    name == "ForgeRock Auth Duration", "3. Auth",
    "Unknown"
)
| summarize avg(value) by bin(timestamp, 5m), metricType
| render areachart   
```

## Optional Dependency Tracking
```cs
public class ForgerockTokenValidator
{
    private readonly TelemetryClient _telemetryClient;

    public ForgerockTokenValidator(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public async Task<bool> IntrospectTokenAsync(string token)
    {
        var dependencyTelemetry = new DependencyTelemetry
        {
            Name = "Forgerock Token Introspection",
            Type = "HTTP",
            Target = "ForgerockAPI",
            Data = "Introspect Token",
            Timestamp = DateTimeOffset.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Perform the Forgerock token introspection
            var isValid = await CallForgerockIntrospectionAsync(token);
            
            // Mark the dependency as successful
            dependencyTelemetry.Success = true;
            return isValid;
        }
        catch (Exception ex)
        {
            // Log exception details to the telemetry
            dependencyTelemetry.Success = false;
            dependencyTelemetry.Properties["Error"] = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            dependencyTelemetry.Duration = stopwatch.Elapsed;

            // Send the telemetry to Application Insights
            _telemetryClient.TrackDependency(dependencyTelemetry);
        }
    }

    private async Task<bool> CallForgerockIntrospectionAsync(string token)
    {
        // Your Forgerock introspection logic here
        await Task.Delay(100); // Simulating the call
        return true;
    }
}
```
