# Serilog + Seq in APIs

## Install
```xml
<!-- LOGGING -->
<PackageReference Include="Serilog" Version="4.0.0" />
<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.1" />
<PackageReference Include="Serilog.Sinks.Async" Version="2.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />
```

## Extension
```csharp
using Serilog;
using Serilog.Filters;

namespace ExpenceTracker.Extensions
{
    public static class SerilogExtensions
    {
        public static IHostBuilder UseSerilogConfiguration(this IHostBuilder builder)
        {
            builder.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore"))
                    .Filter.ByExcluding(Matching.FromSource("Microsoft.Hosting"))
                    .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Mvc"))
                    .WriteTo.Console(outputTemplate: "{Timestamp:dd/MM/yy hh:mm:ss tt} [{Level:u3}] {Message}{NewLine}{Exception}")
                    .WriteTo.Async(x =>
                        x.File(
                                path: "Logs/log.txt",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 5,
                                fileSizeLimitBytes: 20000000,
                                rollOnFileSizeLimit: true,
                                outputTemplate: "{Timestamp:dd/MM/yy hh:mm:ss tt} [{Level:u3}] {Message}{NewLine}{Exception}")
                    )
                    .WriteTo.Async(x =>
                        x.Seq("https://seq.domain.com", apiKey: "**********")
                    );
            });

            return builder;
        }
    }
}
```

## Use Serilog
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilogConfiguration();
```

## Write Logs
```csharp
logger.LogInformation("Validation errors: {@ValidationErrors}", validationResult.ToStandardDictionary());
```
