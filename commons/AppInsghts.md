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

## appsettings.json
```json
{
  "ApplicationInsights": {
    "ConnectionString": " .... "
  },
  "KeyVault": {
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    "VaultUri": "",
    "CacheExpirationMinutes": 60
  }
}
```

## Serilog Setup (Program.cs)
```
using Carter;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.ApplicationInsights;
using parinaybharat.api.Installers.Base;
using Serilog;
using Serilog.Filters;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Application Insights Telemetry configuration to the builder
builder.Services.AddApplicationInsightsTelemetry();

// 2. Configure Serilog as the logging provider for the application
ConfigureLogging(builder);

// 3. Register other services
builder.Services.AddEndpointsApiExplorer();
AutoInstall(builder.Host, builder.Services, builder.Configuration, Assembly.GetExecutingAssembly());

var app = builder.Build();

// 4. Swagger for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapCarter();
app.Run();

static void AutoInstall(IHostBuilder host, IServiceCollection services, IConfiguration configuration, Assembly assembly)
{
    var installers = assembly.GetTypes()
        .Where(t => typeof(IServiceInstaller).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        .Select(Activator.CreateInstance)
        .Cast<IServiceInstaller>()
        .ToList();

    installers.ForEach(installer => installer.InstallService(host, services, configuration));
}

static void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Host.UseSerilog((context, services, config) =>
    {
        config
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
            .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces);
    });
}
```
