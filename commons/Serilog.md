# Serilog Setup & Custom SignalR Sink

### Serilog Packages
```xml
<!-- LOGGING -->
<PackageReference Include="Serilog" Version="4.0.0-dev-02149" />
<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.1-dev-00582" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.1.0-dev-00943" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.1-dev-00972" />
```

### SignalRSink.cs
```csharp
    public class SignalRSink : ILogEventSink
    {
        private readonly SignalRHub hub;

        public SignalRSink(SignalRHub hub)
        {
            this.hub = hub;
        }

        public async void Emit(LogEvent logEvent)
        {
            if (logEvent is not null)
            {
                await hub.BroadcastAsync("Dashboard", "Logs", logEvent.RenderMessage());
            }
        }
    }
```

### SerilogExtensions.cs
```csharp
    public static class SerilogExtensions
    {
        public static IHostBuilder UseSerilogConfiguration(this IHostBuilder builder)
        {
            builder.UseSerilog((context, services, configuration) =>
            {
                var sink = services.GetService<SignalRSink>();
                configuration.ReadFrom.Configuration(context.Configuration);
                configuration.WriteTo.Sink(sink);
            });
            return builder;
        }
    }
```

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

//---------------------------------------------------
builder.Host.UseSerilogConfiguration();
//---------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//---------------------------------------------------

//SignalR
builder.Services.AddSignalR(e =>
{
    e.MaximumReceiveMessageSize = long.MaxValue;
});
builder.Services.AddSingleton<SignalRHub>();

//Serilog Sink
builder.Services.AddSingleton<SignalRSink>();

//---------------------------------------------------

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SignalRHub>("/signalrhub");

app.Run();
```

### appsettings.json

##### The following namespaces are overridden
- Microsoft.AspNetCore.Hosting,
- Microsoft.Hosting.Lifetime,
- Microsoft.AspNetCore.Mvc,
- Microsoft.AspNetCore.Routing.EndpointMiddleware,
- Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker: Warning

```json
"Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore.Hosting": "Warning",
        "Microsoft.Hosting.Lifetime": "Warning",
        "Microsoft.AspNetCore.Mvc": "Warning",
        "Microsoft.AspNetCore.Routing.EndpointMiddleware": "Warning",
        "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:dd/MM/yy hh:mm:ss tt} [{Level:u3}] {Message}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 5,
          "fileSizeLimitBytes": 20000000,
          "rollOnFileSizeLimit": true,
          "outputTemplate": "{Timestamp:dd/MM/yy hh:mm:ss tt} [{Level:u3}] {Message}{NewLine}{Exception}"
        }
      }
    ],
    "Properties": {
      "Application": "app-name"
    }
  }
```
