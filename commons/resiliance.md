# IResilanceService

```csharp
using Polly;

namespace ParinayBharat.Api.Application.Services.Resiliance
{
    public interface IResilientService
    {
        Task<T> ExecuteAsync<T>(Func<Context, Task<T>> action, string serviceName);
        Task ExecuteAsync(Func<Context, Task> action, string serviceName);
    }

}
```

# ResilianceOptions

```csharp
namespace ParinayBharat.Api.Application.Services.Resiliance
{
    public class ResilienceOptions
    {
        public List<ResiliancePolicy> Policies { get; set; }
    }

    public class ResiliancePolicy
    {
        public string Name { get; set; }
        public RetryConfig Retries { get; set; }
        public CircuitBreakingConfig CircuitBreaking { get; set; }
    }

    public class RetryConfig
    {
        public int NoOfRetries { get; set; }
    }

    public class CircuitBreakingConfig
    {
        public int BreakerThreshold { get; set; }
        public int SecondsToRecover { get; set; }
    }

}
```

# ResilianceService

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParinayBharat.Api.Application.Services.Resiliance;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ParinayBharat.Api.Application.Services.Resilience
{
    public class ResilientService : IResilientService
    {
        private readonly ILogger<ResilientService> _logger;
        private readonly ResilienceOptions _resilienceConfig;

        public ResilientService(ILogger<ResilientService> logger, IOptions<ResilienceOptions> resilienceConfig)
        {
            _logger = logger;
            _resilienceConfig = resilienceConfig.Value;
        }

        public async Task<T> ExecuteAsync<T>(Func<Context, Task<T>> action, string serviceName)
        {
            var retryPolicy = GetRetryPolicy(serviceName);
            var circuitBreakerPolicy = GetCircuitBreakerPolicy(serviceName);
            return await retryPolicy.WrapAsync(circuitBreakerPolicy).ExecuteAsync(action, new Context(serviceName));
        }

        public async Task ExecuteAsync(Func<Context, Task> action, string serviceName)
        {
            var retryPolicy = GetRetryPolicy(serviceName);
            var circuitBreakerPolicy = GetCircuitBreakerPolicy(serviceName);
            await retryPolicy.WrapAsync(circuitBreakerPolicy).ExecuteAsync(action, new Context(serviceName));
        }

        private AsyncRetryPolicy GetRetryPolicy(string serviceName)
        {
            var retryConfig = _resilienceConfig.Policies
                .FirstOrDefault(r => r.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                ?.Retries;

            if (retryConfig == null)
            {
                throw new ArgumentException($"No retry configuration found for service '{serviceName}'");
            }

            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(retryConfig.NoOfRetries, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, context) =>
                    {
                        if (!context.ContainsKey("retryCount"))
                        {
                            context["retryCount"] = 0;
                        }
                        context["retryCount"] = (int)context["retryCount"] + 1;
                        var retryCount = (int)context["retryCount"];
                        _logger.LogWarning($"Retrying {retryCount}/{retryConfig.NoOfRetries} - {serviceName} due to: {exception.Message}. Waiting {timeSpan.TotalSeconds} seconds before next retry.");
                    });
        }

        private AsyncCircuitBreakerPolicy GetCircuitBreakerPolicy(string serviceName)
        {
            var circuitConfig = _resilienceConfig.Policies
                .FirstOrDefault(cb => cb.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                ?.CircuitBreaking;

            if (circuitConfig == null)
            {
                throw new ArgumentException($"No circuit breaker configuration found for service '{serviceName}'");
            }

            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(circuitConfig.BreakerThreshold, TimeSpan.FromSeconds(circuitConfig.SecondsToRecover),
                    (exception, duration) =>
                    {
                        _logger.LogError($"Circuit broken for {serviceName} due to: {exception.Message}. Circuit will remain open for {duration.TotalSeconds} seconds.");
                    },
                    () =>
                    {
                        _logger.LogInformation($"Circuit reset for {serviceName}.");
                    },
                    () =>
                    {
                        _logger.LogInformation($"Circuit is half-open for {serviceName}.");
                    });
        }
    }
}
```

# Injection

```csharp
            //Resilance
            services.Configure<ResilienceOptions>(configuration.GetSection("ResilienceOptions"));
            services.AddTransient<IResilientService, ResilientService>();
```

# appsettings.json

```json
"ResilienceOptions": {
  "Policies": [
    {
      "Name": "ManagedInstance",
      "Retries": {
        "NoOfRetries": 3
      },
      "CircuitBreaking": {
        "BreakerThreshold": 3,
        "SecondsToRecover": 1
      }
    },
    {
      "Name": "ForgeRock",
      "Retries": {
        "NoOfRetries": 3
      },
      "CircuitBreaking": {
        "BreakerThreshold": 3,
        "SecondsToRecover": 1
      }
    }
  ]
}
```

# appsettings.json

```json
"ResilienceOptions": {
  "Policies": [
    {
      "Name": "ManagedInstance",
      "Retries": {
        "NoOfRetries": 3
      },
      "CircuitBreaking": {
        "BreakerThreshold": 3,
        "SecondsToRecover": 1
      }
    },
    {
      "Name": "ForgeRock",
      "Retries": {
        "NoOfRetries": 3
      },
      "CircuitBreaking": {
        "BreakerThreshold": 3,
        "SecondsToRecover": 1
      }
    }
  ]
}
```
