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
        Task<T> RunAndTrackMetricsAsync<T>(Func<Task<T>> context, MetricName metricName);
        Task RunAndTrackMetricsAsync<T>(Func<Task> context, MetricName metricName);
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

### Metric Names
```cs
namespace parinaybharat.api.application.Helpers.Telemetry
{
    public enum MetricName
    {
        TOTAL_REQUEST_TIME,
        TOTAL_DATABASE_TIME,
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
using CorrelationId.Abstractions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Application.Helpers.Telemetry
{
    public class TelemetryService : ITelemetryService
    {
        private readonly IConfiguration configuration;
        private readonly TelemetryClient telemetryClient;
        private readonly ICorrelationContextAccessor correlationContext;

        public TelemetryService(TelemetryClient telemetryClient, ICorrelationContextAccessor correlationContext, IConfiguration configuration)
        {
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void TrackEvent(string name, IDictionary<string, string> properties = null)
        {
            properties ??= new Dictionary<string, string>();
            properties["AppName"] = configuration.GetValue<string>("AppName", "DefaultAppName");
            telemetryClient.TrackEvent(name, properties);
        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            telemetryClient.TrackMetric(name, value, properties ?? new Dictionary<string, string>());
        }

        public string TrackChainedDependency(
            string name,
            TimeSpan duration,
            bool success,
            string target = null,
            string dependencyType = DependencyType.Http,
            string parentId = null,
            string operationId = null)
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
                dependencyTelemetry.Context.Operation.ParentId = parentId;
            if (!string.IsNullOrEmpty(operationId))
                dependencyTelemetry.Context.Operation.Id = operationId;

            telemetryClient.TrackDependency(dependencyTelemetry);
            return dependencyTelemetry.Id;
        }

        public void TrackException(Exception exception, string requestId, IDictionary<string, string> properties = null)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            if (string.IsNullOrEmpty(requestId)) throw new ArgumentException("RequestId cannot be null or empty", nameof(requestId));

            var propertiesWithRequestId = properties ?? new Dictionary<string, string>();
            propertiesWithRequestId["RequestId"] = requestId;
            telemetryClient.TrackException(exception, propertiesWithRequestId);
        }

        public async Task<T> RunAndTrackMetricsAsync<T>(Func<Task<T>> context, string metricName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await context();
            }
            finally
            {
                stopwatch.Stop();
                TrackMetric(metricName, stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "RequestId", correlationContext.CorrelationContext?.CorrelationId ?? "Unknown" }
                });
            }
        }

        public async Task RunAndTrackMetricsAsync(Func<Task> context, string metricName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await context();
            }
            finally
            {
                stopwatch.Stop();
                TrackMetric(metricName, stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "RequestId", correlationContext.CorrelationContext?.CorrelationId ?? "Unknown" }
                });
            }
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

## Unit Tests
```cs
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CorrelationId.Abstractions;

namespace UnitTests.Helpers.Telemetry
{
    public class TelemetryServiceTests
    {
        private readonly Mock<TelemetryClient> telemetryClientMock = new();
        private readonly Mock<ICorrelationContextAccessor> correlationContextMock = new();
        private readonly Mock<IConfiguration> configurationMock = new();
        private readonly TelemetryService telemetryService;

        public TelemetryServiceTests()
        {
            configurationMock.Setup(c => c["AppName"]).Returns("TestApp");
            correlationContextMock.Setup(c => c.CorrelationContext.CorrelationId).Returns("TestCorrelationId");
            telemetryService = new TelemetryService(telemetryClientMock.Object, correlationContextMock.Object, configurationMock.Object);
        }

        [Fact]
        public void TrackEvent_ShouldAddAppNameToProperties()
        {
            var properties = new Dictionary<string, string>();
            telemetryService.TrackEvent("TestEvent", properties);

            Assert.True(properties.ContainsKey("AppName"));
            Assert.Equal("TestApp", properties["AppName"]);
            telemetryClientMock.Verify(t => t.TrackEvent("TestEvent", properties), Times.Once);
        }

        [Fact]
        public void TrackMetric_ShouldTrackMetricWithProperties()
        {
            telemetryService.TrackMetric("TestMetric", 1.0, null);

            telemetryClientMock.Verify(t => t.TrackMetric("TestMetric", 1.0, It.IsAny<IDictionary<string, string>>()), Times.Once);
        }

        [Fact]
        public void TrackChainedDependency_ShouldSetDependencyProperties()
        {
            var dependencyId = telemetryService.TrackChainedDependency("TestDependency", TimeSpan.FromSeconds(1), true);

            telemetryClientMock.Verify(t => t.TrackDependency(It.Is<DependencyTelemetry>(d => d.Name == "TestDependency" && d.Success == true)));
            Assert.NotNull(dependencyId);
        }

        [Fact]
        public void TrackException_ShouldTrackExceptionWithProperties()
        {
            var properties = new Dictionary<string, string>();
            telemetryService.TrackException(new Exception("TestException"), "TestRequestId", properties);

            Assert.True(properties.ContainsKey("RequestId"));
            Assert.Equal("TestRequestId", properties["RequestId"]);
            telemetryClientMock.Verify(t => t.TrackException(It.IsAny<Exception>(), properties), Times.Once);
        }

        [Fact]
        public async Task RunAndTrackMetricsAsync_T_ShouldTrackMetric()
        {
            await telemetryService.RunAndTrackMetricsAsync(async () => { await Task.Delay(1); return 1; }, "TestMetric");

            telemetryClientMock.Verify(t => t.TrackMetric("TestMetric", It.IsAny<double>(), It.IsAny<IDictionary<string, string>>()), Times.Once);
        }

        [Fact]
        public async Task RunAndTrackMetricsAsync_ShouldTrackMetric()
        {
            await telemetryService.RunAndTrackMetricsAsync(async () => await Task.Delay(1), "TestMetric");

            telemetryClientMock.Verify(t => t.TrackMetric("TestMetric", It.IsAny<double>(), It.IsAny<IDictionary<string, string>>()), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenTelemetryClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TelemetryService(null, correlationContextMock.Object, configurationMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenCorrelationContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TelemetryService(telemetryClientMock.Object, null, configurationMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TelemetryService(telemetryClientMock.Object, correlationContextMock.Object, null));
        }
    }
}
```
