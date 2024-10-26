# AppInsghts Metrics and KQL
![image](https://github.com/user-attachments/assets/0fb187a2-ae67-4278-bcbf-c9a184e79a3a)

![image](https://github.com/user-attachments/assets/23492fd2-ca3e-4cef-aec1-eead34536544)

## Setup TelemetryService

### ITelemetryService
```cs
namespace parinaybharat.api.application.Helpers.Telemetry
{
    public interface ITelemetryService
    {
        void TrackEvent(string name, IDictionary<string, string> properties = null);
        void TrackMetric(string name, double value, IDictionary<string, string> properties = null);
        string TrackChainedDependency(
            string name,
            TimeSpan duration,
            bool success,
            string target = null,
            string dependencyType = DependencyType.HTTP,
            string parentId = null,
            string operationId = null);
        void TrackException(Exception exception, string requestId, IDictionary<string, string> properties = null);
    }
}

```

### KQL
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

### TelemetryService
```cs
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;

namespace parinaybharat.api.application.Helpers.Telemetry
{
    public class TelemetryService : ITelemetryService
    {
        private readonly TelemetryClient _telemetryClient;

        public TelemetryService(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackEvent(string name, IDictionary<string, string> properties = null)
        {
            _telemetryClient.TrackEvent(name, properties);
        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            _telemetryClient.TrackMetric(name, value, properties);
        }

        public string TrackChainedDependency(
            string name,
            TimeSpan duration,
            bool success,
            string target = null,
            string dependencyType = DependencyType.HTTP,
            string parentId = null,
            string operationId = null) // New parameter for operationId
        {
            var dependencyTelemetry = new DependencyTelemetry
            {
                Name = name,
                Type = dependencyType.ToString(),
                Target = target ?? "Unknown",
                Data = "Chained dependency call",
                Duration = duration,
                Success = success,
                Timestamp = DateTimeOffset.UtcNow
            };

            if (!string.IsNullOrEmpty(parentId))
            {
                dependencyTelemetry.Context.Operation.ParentId = parentId; // Set parent ID for chaining
            }

            if (!string.IsNullOrEmpty(operationId))
            {
                dependencyTelemetry.Context.Operation.Id = operationId; // Set operation ID for overall request
            }

            _telemetryClient.TrackDependency(dependencyTelemetry);
            return dependencyTelemetry.Id; // Return ID to be used as ParentId for the next dependency
        }


        public void TrackException(Exception exception, string requestId, IDictionary<string, string> properties = null)
        {
            var propertiesWithRequestId = properties ?? new Dictionary<string, string>();
            propertiesWithRequestId["RequestId"] = requestId;
            _telemetryClient.TrackException(exception, propertiesWithRequestId);
        }
    }
}
```

### Dependency Types Azure Supports
```cs
namespace parinaybharat.api.application.Helpers.Telemetry
{
    public static class DependencyType
    {
        public const string SQL = "SQL";
        public const string HTTP = "HTTP";
        public const string AzureBlob = "Azure Blob";
        public const string AzureTable = "Azure Table";
        public const string AzureQueue = "Azure Queue";
        public const string WebService = "Web Service";
        public const string WCFService = "WCF Service";
        public const string AJAX = "AJAX";
        public const string Other = "Other";
    }
}
```

## Sample Usage

```cs
using Microsoft.AspNetCore.Http;
using parinaybharat.api.application.Helpers.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace parinaybharat.api.presentation.Middlewares
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITelemetryService _telemetryService;

        public RequestTimingMiddleware(RequestDelegate next, ITelemetryService telemetryService)
        {
            _next = next;
            _telemetryService = telemetryService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            string requestId = Guid.NewGuid().ToString();
            string parentDependencyId = null;

            try
            {
                // HTTP dependency example
                var dependencyDuration = TimeSpan.FromMilliseconds(new Random().Next(50, 200));
                bool dependencySuccess = new Random().Next(0, 10) > 2;


                // First dependency
                var firstDependencyDuration = TimeSpan.FromMilliseconds(new Random().Next(50, 200));
                parentDependencyId = _telemetryService.TrackChainedDependency("ForgerockAuthResolution",
                                                                              firstDependencyDuration,
                                                                              success: true,
                                                                              target: "Forgerock Introspection",
                                                                              dependencyType: DependencyType.HTTP,
                                                                              "API Flow");

                // Second dependency in chain, using the first's ID as parent
                var secondDependencyDuration = firstDependencyDuration.Add(TimeSpan.FromMilliseconds(20));
                parentDependencyId = _telemetryService.TrackChainedDependency("ManagedInstanceQuery",
                                                                              secondDependencyDuration,
                                                                              success: false,
                                                                              target: "SQL Database",
                                                                              dependencyType: DependencyType.SQL,
                                                                              parentId: parentDependencyId,
                                                                              "API Flow");

                // Third dependency in chain, using the second's ID as parent
                var thirdDependencyDuration = secondDependencyDuration.Add(TimeSpan.FromMilliseconds(30));
                _telemetryService.TrackChainedDependency("ExternalApiCall",
                                                         thirdDependencyDuration,
                                                         success: true,
                                                         target: "External Service",
                                                         dependencyType: DependencyType.AzureBlob,
                                                         parentId: parentDependencyId,
                                                         "API Flow");


                await _next(context);

                // Track a successful request event
                _telemetryService.TrackEvent("RequestProcessed", new Dictionary<string, string>
                {
                    { "RequestId", requestId },
                    { "EndpointName", context.Request.Path },
                    { "Method", context.Request.Method }
                });
            }
            catch (Exception ex)
            {
                // Track any exceptions encountered
                _telemetryService.TrackException(ex, requestId);
            }
            finally
            {
                stopwatch.Stop();
                var endpoint = context.GetEndpoint();
                var endpointName = endpoint?.DisplayName ?? context.Request.Path;

                // Track the total duration of the request
                _telemetryService.TrackMetric("Request Duration", stopwatch.Elapsed.TotalMilliseconds, new Dictionary<string, string>
                {
                    { "EndpointName", endpointName },
                    { "Method", context.Request.Method },
                    { "StatusCode", context.Response.StatusCode.ToString() },
                    { "RequestId", requestId }
                });
            }
        }
    }
}
```
