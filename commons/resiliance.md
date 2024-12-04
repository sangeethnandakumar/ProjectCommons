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

# Xunit

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Polly;
using Polly.CircuitBreaker;
using Xunit;
using ParinayBharat.Api.Application.Services.Resilience;
using ParinayBharat.Api.Application.Services.Resiliance;

namespace ParinayBharat.Api.Tests.Services
{
    public class ResilientServiceTests
    {
        private readonly Mock<ILogger<ResilientService>> _mockLogger;
        private readonly Mock<IOptions<ResilienceOptions>> _mockResilienceOptions;
        private readonly ResilientService _resilientService;
        private readonly ResilienceOptions _resilienceOptions;

        public ResilientServiceTests()
        {
            _mockLogger = new Mock<ILogger<ResilientService>>();
            _resilienceOptions = new ResilienceOptions
            {
                Policies = new[]
                {
                    new ResiliencePolicy
                    {
                        Name = "TestService",
                        Retries = new RetryConfig
                        {
                            NoOfRetries = 3
                        },
                        CircuitBreaking = new CircuitBreakerConfig
                        {
                            BreakerThreshold = 3,
                            SecondsToRecover = 30
                        }
                    }
                }
            };

            _mockResilienceOptions = new Mock<IOptions<ResilienceOptions>>();
            _mockResilienceOptions.Setup(x => x.Value).Returns(_resilienceOptions);

            _resilientService = new ResilientService(_mockLogger.Object, _mockResilienceOptions.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WithReturnValue_SuccessfulExecution_ReturnsResult()
        {
            // Arrange
            int expectedResult = 42;
            Func<Context, Task<int>> action = context => Task.FromResult(expectedResult);

            // Act
            var result = await _resilientService.ExecuteAsync(action, "TestService");

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithoutReturnValue_SuccessfulExecution()
        {
            // Arrange
            bool wasExecuted = false;
            Func<Context, Task> action = context =>
            {
                wasExecuted = true;
                return Task.CompletedTask;
            };

            // Act
            await _resilientService.ExecuteAsync(action, "TestService");

            // Assert
            Assert.True(wasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_WithReturnValue_RetriesOnException()
        {
            // Arrange
            int callCount = 0;
            Func<Context, Task<int>> action = context =>
            {
                callCount++;
                if (callCount < 3)
                {
                    throw new InvalidOperationException("Test exception");
                }
                return Task.FromResult(42);
            };

            // Act
            var result = await _resilientService.ExecuteAsync(action, "TestService");

            // Assert
            Assert.Equal(42, result);
            Assert.Equal(3, callCount);
        }

        [Fact]
        public async Task ExecuteAsync_WithoutReturnValue_RetriesOnException()
        {
            // Arrange
            int callCount = 0;
            Func<Context, Task> action = context =>
            {
                callCount++;
                if (callCount < 3)
                {
                    throw new InvalidOperationException("Test exception");
                }
                return Task.CompletedTask;
            };

            // Act
            await _resilientService.ExecuteAsync(action, "TestService");

            // Assert
            Assert.Equal(3, callCount);
        }

        [Fact]
        public async Task ExecuteAsync_WithReturnValue_ThrowsAfterMaxRetries()
        {
            // Arrange
            Func<Context, Task<int>> action = context =>
            {
                throw new InvalidOperationException("Persistent exception");
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _resilientService.ExecuteAsync(action, "TestService"));
        }

        [Fact]
        public async Task ExecuteAsync_WithoutReturnValue_ThrowsAfterMaxRetries()
        {
            // Arrange
            Func<Context, Task> action = context =>
            {
                throw new InvalidOperationException("Persistent exception");
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _resilientService.ExecuteAsync(action, "TestService"));
        }

        [Fact]
        public void GetRetryPolicy_WithMissingServiceConfiguration_ThrowsArgumentException()
        {
            // Arrange
            var emptyOptions = new ResilienceOptions { Policies = new ResiliencePolicy[0] };
            var mockEmptyOptions = new Mock<IOptions<ResilienceOptions>>();
            mockEmptyOptions.Setup(x => x.Value).Returns(emptyOptions);

            var service = new ResilientService(_mockLogger.Object, mockEmptyOptions.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
            {
                // Use reflection to call private method
                var method = typeof(ResilientService).GetMethod("GetRetryPolicy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(service, new object[] { "NonExistentService" });
            });
        }

        [Fact]
        public void GetCircuitBreakerPolicy_WithMissingServiceConfiguration_ThrowsArgumentException()
        {
            // Arrange
            var emptyOptions = new ResilienceOptions { Policies = new ResiliencePolicy[0] };
            var mockEmptyOptions = new Mock<IOptions<ResilienceOptions>>();
            mockEmptyOptions.Setup(x => x.Value).Returns(emptyOptions);

            var service = new ResilientService(_mockLogger.Object, mockEmptyOptions.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
            {
                // Use reflection to call private method
                var method = typeof(ResilientService).GetMethod("GetCircuitBreakerPolicy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(service, new object[] { "NonExistentService" });
            });
        }
    }
}
```
