# AppInsghts Metrics and KQL
![image](https://github.com/user-attachments/assets/0fb187a2-ae67-4278-bcbf-c9a184e79a3a)

![image](https://github.com/user-attachments/assets/23492fd2-ca3e-4cef-aec1-eead34536544)

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

<hr/>

## 1. Create A Wrapper TelemetryClient (As it's a sealed class and can't UnitTest)

### ITelemetryClient
```cs
    public interface ITelemetryClient
    {
        void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrices = null);
        void TrackMetric(string metricName, double value, IDictionary<string, string> properties = null);
        void TrackDependency(DependencyTelemetry dependencyTelemetry);
        void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrices = null);
    }
```

### TelemetryClientWrapper
```cs
public class TelemetryClientWrapper : ITelemetryClient
{
    private readonly TelemetryClient _telemetryClient;

    public TelemetryClientWrapper(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrices = null)
    {
        _telemetryClient.TrackEvent(eventName, properties, metrices);
    }

    public void TrackMetric(string metricName, double value, IDictionary<string, string> properties = null)
    {
        _telemetryClient.TrackMetric(metricName, value, properties);
    }

    public void TrackDependency(DependencyTelemetry dependencyTelemetry)
    {
        _telemetryClient.TrackDependency(dependencyTelemetry);
    }

    public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrices = null)
    {
        _telemetryClient.TrackException(exception, properties, metrices);
    }
}
```

<hr/>

## 2. Create Telemetry Service

### ITelemetryService
```cs
public interface ITelemetryService
{
    void TrackEvent(string name, IDictionary<string, string>? properties = null);
    void TrackMetric(string name, double value, IDictionary<string, string>? properties = null);
    Task<T> RunAndTrackMetricsAsync<T>(Func<Task<T>> context, MetricName metricName);
    Task RunAndTrackMetricsAsync(Func<Task> context, MetricName metricName);
    string TrackDependency(
        string name,
        TimeSpan duration,
        bool success,
        string target = null,
        string dependencyType = DependencyType.Http,
        string parentId = null,
        string operationId = null);
    void TrackException(Exception exception, string requestId, IDictionary<string, string>? properties = null);
}
```

### Metric Names
```cs
    public enum MetricName
    {
        TOTAL_REQUEST_TIME,
        TOTAL_DATABASE_TIME,
    }
```

### Dependency Types Azure Supports
```cs
    public static class DependencyType
    {
        public const string SQL = "SQL";
        public const string HTTP = "Http";
        public const string AzureBlob = "Azure Blob";
        public const string AzureTable = "Azure Table";
        public const string AzureQueue = "Azure Queue";
        public const string WebService = "Web Service";
        public const string WCFService = "WCF Service";
        public const string AJAX = "AJAX";
        public const string Other = "Other";
    }
```

### TelemetryService
```cs
public class TelemetryService : ITelemetryService
{
    private readonly IConfiguration configuration;
    private readonly ITelemetryClient telemetryClient;
    private readonly ICorrelationContextAccessor correlationContext;

    public TelemetryService(ITelemetryClient telemetryClient, ICorrelationContextAccessor correlationContext, IConfiguration configuration)
    {
        this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        this.correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void TrackEvent(string name, IDictionary<string, string>? properties = null)
    {
        var appName = configuration["AppName"];
        if (string.IsNullOrEmpty(appName))
        {
            throw new InvalidOperationException("AppName is not configured.");
        }

        properties ??= new Dictionary<string, string>();
        properties["AppName"] = appName;
        telemetryClient.TrackEvent(name, properties);
    }


    public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
    {
        telemetryClient.TrackMetric(name, value, properties ?? new Dictionary<string, string>());
    }

    public string TrackDependency(
        string name,
        TimeSpan duration,
        bool success,
        string? target = null,
        string dependencyType = DependencyType.Http,
        string? parentId = null,
        string? operationId = null)
    {
        var dependencyTelemetry = new DependencyTelemetry
        {
            Name = name,
            Type = dependencyType.ToString(),
            Target = target ?? "Unknown",
            Data = "Dependency call",
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

    public void TrackException(Exception exception, string requestId, IDictionary<string, string>? properties = null)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        if (string.IsNullOrEmpty(requestId)) throw new ArgumentException("RequestId cannot be null or empty", nameof(requestId));

        var propertiesWithRequestId = properties ?? new Dictionary<string, string>();
        propertiesWithRequestId["RequestId"] = requestId;
        telemetryClient.TrackException(exception, propertiesWithRequestId);
    }

    public async Task<T> RunAndTrackMetricsAsync<T>(Func<Task<T>> context, MetricName metricName)
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
            TrackMetric(metricName.ToString(), stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
            {
                { "RequestId", correlationContext.CorrelationContext?.CorrelationId ?? "Unknown" }
            });
        }
    }

    public async Task RunAndTrackMetricsAsync(Func<Task> context, MetricName metricName)
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
            TrackMetric(metricName.ToString(), stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
            {
                { "RequestId", correlationContext.CorrelationContext?.CorrelationId ?? "Unknown" }
            });
        }
    }
}
```

<hr/>


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
                // First dependency example
                var firstDependencyDuration = TimeSpan.FromMilliseconds(new Random().Next(50, 200));
                parentDependencyId = _telemetryService.TrackDependency(
                    "ForgerockAuthResolution",
                    firstDependencyDuration,
                    success: true,
                    target: "Forgerock Introspection",
                    dependencyType: DependencyType.Http,
                    parentId: null,
                    operationId: "API Flow");

                // Second dependency in chain, using the first's ID as parent
                var secondDependencyDuration = firstDependencyDuration.Add(TimeSpan.FromMilliseconds(20));
                parentDependencyId = _telemetryService.TrackDependency(
                    "ManagedInstanceQuery",
                    secondDependencyDuration,
                    success: false,
                    target: "SQL Database",
                    dependencyType: DependencyType.Sql,
                    parentId: parentDependencyId,
                    operationId: "API Flow");

                // Third dependency in chain, using the second's ID as parent
                var thirdDependencyDuration = secondDependencyDuration.Add(TimeSpan.FromMilliseconds(30));
                _telemetryService.TrackDependency(
                    "ExternalApiCall",
                    thirdDependencyDuration,
                    success: true,
                    target: "External Service",
                    dependencyType: DependencyType.AzureBlob,
                    parentId: parentDependencyId,
                    operationId: "API Flow");

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

<hr/>

## Unit Tests
```cs
public class TelemetryServiceTests
{
    private readonly Mock<ITelemetryClient> _mockTelemetryClient;
    private readonly Mock<ICorrelationContextAccessor> _mockCorrelationContextAccessor;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly TelemetryService _telemetryService;

    public TelemetryServiceTests()
    {
        _mockTelemetryClient = new Mock<ITelemetryClient>();
        _mockCorrelationContextAccessor = new Mock<ICorrelationContextAccessor>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockCorrelationContextAccessor.Setup(x => x.CorrelationContext).Returns(new CorrelationContext("test-correlation-id", "x-correlation-id"));
        _telemetryService = new TelemetryService(_mockTelemetryClient.Object, _mockCorrelationContextAccessor.Object, _mockConfiguration.Object);
    }

    [Fact]
    public void TrackEvent_ShouldThrowInvalidOperationException_WhenAppNameIsNotConfigured()
    {
        _mockConfiguration.Setup(config => config["AppName"]).Returns((string?)null);
        var properties = new Dictionary<string, string>();

        Assert.Throws<InvalidOperationException>(() => _telemetryService.TrackEvent("test-event", properties));
    }

    [Fact]
    public void TrackEvent_ShouldTrackEvent_WhenAppNameIsConfigured()
    {
        // Arrange
        _mockConfiguration.Setup(config => config["AppName"]).Returns("MyApp");
        var properties = new Dictionary<string, string>();

        // Act
        _telemetryService.TrackEvent("test-event", properties);

        // Add the expected AppName to the properties
        properties["AppName"] = "MyApp";

        // Assert
        _mockTelemetryClient.Verify(client => client.TrackEvent(
            "test-event",
            It.Is<IDictionary<string, string>>(dict => dict.ContainsKey("AppName") && dict["AppName"] == "MyApp"),
            null),
            Times.Once);
    }

    [Fact]
    public void TrackMetric_ShouldTrackMetric_WhenCalled()
    {
        var properties = new Dictionary<string, string> { { "key", "value" } };

        _telemetryService.TrackMetric("test-metric", 123.45, properties);

        _mockTelemetryClient.Verify(client => client.TrackMetric("test-metric", 123.45, It.IsAny<IDictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public void TrackDependency_ShouldTrackDependency_WithCorrectTelemetry()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(100);
        var dependencyName = "dependency";
        var dependencyType = "SQL";
        var target = "target-system";
        var parentId = "parent-id";
        var operationId = "operation-id";

        // Act
        var telemetryId = _telemetryService.TrackDependency(dependencyName, duration, true, target, dependencyType, parentId, operationId);

        // Assert
        _mockTelemetryClient.Verify(client => client.TrackDependency(It.Is<DependencyTelemetry>(dep =>
            dep.Name == dependencyName &&
            dep.Type == dependencyType &&
            dep.Target == target &&
            dep.Duration == duration &&
            dep.Success == true &&
            dep.Context.Operation.ParentId == parentId &&
            dep.Context.Operation.Id == operationId)), Times.Once);

        // Assert that a telemetry ID was generated
        Assert.NotNull(telemetryId);
    }


    [Fact]
    public void TrackException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _telemetryService.TrackException(null, "requestId"));
    }

    [Fact]
    public void TrackException_ShouldThrowArgumentException_WhenRequestIdIsEmpty()
    {
        var exception = new Exception("Test exception");

        Assert.Throws<ArgumentException>(() => _telemetryService.TrackException(exception, string.Empty));
    }

    [Fact]
    public void TrackException_ShouldTrackException_WithRequestId()
    {
        var exception = new Exception("Test exception");
        var properties = new Dictionary<string, string>();

        _telemetryService.TrackException(exception, "request-id", properties);

        properties["RequestId"] = "request-id";
        _mockTelemetryClient.Verify(client => client.TrackException(exception, It.IsAny<IDictionary<string, string>>(), null), Times.Once);
    }

    [Fact]
    public async Task RunAndTrackMetricsAsync_ShouldTrackMetric_WhenFuncTaskTCompletes()
    {
        var metricName = MetricName.TOTAL_DATABASE_TIME;
        var result = "test-result";
        Func<Task<string>> func = () => Task.FromResult(result);

        var returnValue = await _telemetryService.RunAndTrackMetricsAsync(func, metricName);

        Assert.Equal(result, returnValue);
        _mockTelemetryClient.Verify(client => client.TrackMetric(metricName.ToString(), It.IsAny<double>(), It.Is<Dictionary<string, string>>(dict =>
            dict.ContainsKey("RequestId") && dict["RequestId"] == "test-correlation-id")), Times.Once);
    }

    [Fact]
    public async Task RunAndTrackMetricsAsync_ShouldTrackMetric_WhenFuncTaskCompletes()
    {
        var metricName = MetricName.TOTAL_DATABASE_TIME;
        Func<Task> func = () => Task.CompletedTask;

        await _telemetryService.RunAndTrackMetricsAsync(func, metricName);

        _mockTelemetryClient.Verify(client => client.TrackMetric(metricName.ToString(), It.IsAny<double>(), It.Is<Dictionary<string, string>>(dict =>
            dict.ContainsKey("RequestId") && dict["RequestId"] == "test-correlation-id")), Times.Once);
    }

    [Fact]
    public async Task RunAndTrackMetricsAsync_ShouldThrowArgumentNullException_WhenFuncTaskTIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _telemetryService.RunAndTrackMetricsAsync<string>(null!, MetricName.TOTAL_DATABASE_TIME));
    }

    [Fact]
    public async Task RunAndTrackMetricsAsync_ShouldThrowArgumentNullException_WhenFuncTaskIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _telemetryService.RunAndTrackMetricsAsync(null!, MetricName.TOTAL_DATABASE_TIME));
    }
}
```
