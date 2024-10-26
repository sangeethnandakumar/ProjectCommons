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
