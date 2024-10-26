# Configure Serilog + Azure App Insghts

## Install
```xml
	<ItemGroup>
		<!-- SERILOG LOGGING -->
		<PackageReference Include="Serilog" Version="4.1.0" />
		<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
		<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.4" />
		<PackageReference Include="Serilog.Sinks.Async" Version="2.1.0" />
		<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
		<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
		<PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />
		<PackageReference Include="SixLabors.ImageSharp" Version="3.1.5" />
		<!-- APP INSGHTS INTEGRATION -->
		<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
		<PackageReference Include="Serilog.Sinks.ApplicationInsights" Version="4.0.0" />
		<!--CONTAINERISATION-->
		<PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" Version="1.21.0" />
	</ItemGroup>
```

## Add Logger Extensions
```cs
using Microsoft.ApplicationInsights;
using Serilog;
using Serilog.Filters;

namespace parinaybharat.api.Extensions
{
    public static class LoggerExtensions
    {
        public static void ConfigureApplicationLogging(this WebApplicationBuilder builder)
        {
            // Bind the configuration
            var loggerConfig = new LoggerConfig();
            builder.Configuration.GetSection("LoggerConfiguration").Bind(loggerConfig);

            // Setup Serilog with the custom configuration
            builder.Host.UseSerilog((context, services, config) =>
            {
                config.MinimumLevel.Information()
                    .Enrich.FromLogContext();

                // Apply exclusions
                foreach (var exclusion in loggerConfig.Exclusions)
                {
                    config.Filter.ByExcluding(Matching.FromSource(exclusion));
                }

                // Configure Console sink
                if (loggerConfig.Sinks.Console.Enable)
                {
                    config.MinimumLevel.Is((Serilog.Events.LogEventLevel)Enum.Parse(typeof(Serilog.Events.LogEventLevel), loggerConfig.Sinks.Console.MinimumLevel.ToString()));
                    config.WriteTo.Console(outputTemplate: loggerConfig.Sinks.Console.Template);
                }

                // Configure File sink
                if (loggerConfig.Sinks.File.Enable)
                {
                    config.MinimumLevel.Is((Serilog.Events.LogEventLevel)Enum.Parse(typeof(Serilog.Events.LogEventLevel), loggerConfig.Sinks.File.MinimumLevel.ToString()));
                    config.WriteTo.Async(x =>
                        x.File(
                            path: loggerConfig.Sinks.File.Path,
                            rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), loggerConfig.Sinks.File.RollingInterval.ToString()),
                            retainedFileCountLimit: loggerConfig.Sinks.File.RetainedFileCountLimit,
                            fileSizeLimitBytes: loggerConfig.Sinks.File.FileSizeLimitBytes,
                            rollOnFileSizeLimit: loggerConfig.Sinks.File.RollOnFileSizeLimit,
                            outputTemplate: loggerConfig.Sinks.File.OutputTemplate)
                    );
                }

                // Configure Application Insights sink
                if (loggerConfig.Sinks.AzureAppInsights.Enable)
                {
                    config.MinimumLevel.Is((Serilog.Events.LogEventLevel)Enum.Parse(typeof(Serilog.Events.LogEventLevel), loggerConfig.Sinks.AzureAppInsights.MinimumLevel.ToString()));
                    config.WriteTo.ApplicationInsights(
                        services.GetRequiredService<TelemetryClient>(),
                        TelemetryConverter.Traces);
                }
            });
        }
    }
}
```

## Add IOC Extensions
```cs
using parinaybharat.api.Installers.Base;
using System.Reflection;

namespace parinaybharat.api.Extensions
{
    public static class IocExtensions
    {
        public static void AutoInstallDependencies(this IHostBuilder host, IServiceCollection services, IConfiguration configuration, Assembly assembly)
        {
            var installers = assembly.GetTypes()
                .Where(t => typeof(IServiceInstaller).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IServiceInstaller>()
                .ToList();

            installers.ForEach(installer => installer.InstallService(host, services, configuration));
        }
    }
}
```

## appsettings.json
```json
{
  "ApplicationInsights": {
    "ConnectionString": ""
  },
  "KeyVault": {
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    "VaultUri": "",
    "CacheExpirationMinutes": 60
  },
  "LoggerConfiguration": {
    "Exclusions": [
      "Microsoft.AspNetCore",
      "Microsoft.Hosting",
      "Microsoft.AspNetCore.Mvc"
    ],
    "Sinks": {
      "Console": {
        "Enable": true,
        "MinimumLevel": "Information",
        "Template": "{Timestamp:dd/MM/yy hh:mm:ss tt} [{Level:u3}] {Message}{NewLine}{Exception}"
      },
      "File": {
        "Enable": true,
        "MinimumLevel": "Information",
        "Path": "Logs/log.txt",
        "RollingInterval": "Day",
        "RetainedFileCountLimit": 5,
        "FileSizeLimitBytes": 20000000,
        "RollOnFileSizeLimit": true,
        "OutputTemplate": "{Timestamp:dd/MM/yy hh:mm:ss tt} [{Level:u3}] {Message}{NewLine}{Exception}"
      },
      "AzureAppInsights": {
        "Enable": true,
        "MinimumLevel": "Information"
      }
    }
  }
}
```

## LogConfig Binding Model
```cs
namespace parinaybharat.api.Extensions
{
    public class LoggerConfig
    {
        public List<string> Exclusions { get; set; } = new();
        public SinkConfig Sinks { get; set; } = new();
    }

    public class SinkConfig
    {
        public ConsoleSink Console { get; set; } = new();
        public FileSink File { get; set; } = new();
        public AppInsightsSink AzureAppInsights { get; set; } = new();
    }

    public class ConsoleSink
    {
        public bool Enable { get; set; }
        public string Template { get; set; }
        public LogLevel MinimumLevel { get; set; }
    }

    public class FileSink
    {
        public bool Enable { get; set; }
        public string Path { get; set; }
        public RollingIntervalEnum RollingInterval { get; set; }
        public int RetainedFileCountLimit { get; set; }
        public int FileSizeLimitBytes { get; set; }
        public bool RollOnFileSizeLimit { get; set; }
        public string OutputTemplate { get; set; }
        public LogLevel MinimumLevel { get; set; }
    }

    public class AppInsightsSink
    {
        public bool Enable { get; set; }
        public LogLevel MinimumLevel { get; set; }
    }

    // Enums for configuration options
    public enum RollingIntervalEnum
    {
        Infinite,
        Year,
        Month,
        Day,
        Hour,
        Minute
    }

    public enum LogLevel
    {
        Verbose,
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }
}
```

## Program.cs
```cs
using Carter;
using parinaybharat.api.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Application Insights Telemetry configuration to the builder
builder.Services.AddApplicationInsightsTelemetry();

// 2. Configure Serilog as the logging provider for the application
builder.ConfigureApplicationLogging();

// 3. Register other services
builder.Host.AutoInstallDependencies(builder.Services, builder.Configuration, Assembly.GetExecutingAssembly());

// 4. Add API Explorer
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapCarter();
app.Run();
```
