## Solution Structuring

- Create 5 projects
  - API
  - Domain
  - Application
  - Presentation
  - Persistance

# Dependency Graph

### API
```xml
	<ItemGroup>
		<!-- SERILOG LOGGING -->
		<PackageReference Include="Serilog" Version="4.0.2" />
		<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
		<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.4" />
		<PackageReference Include="Serilog.Sinks.Async" Version="2.0.0" />
		<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
		<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
		<PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />
		<PackageReference Include="SixLabors.ImageSharp" Version="3.1.5" />
		<!--CONTAINERISATION-->
		<PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" Version="1.21.0" />
	</ItemGroup>

	<ItemGroup>
		<ProjectReference Include="..\parinaybharat.api.application\parinaybharat.api.application.csproj" />
		<ProjectReference Include="..\parinaybharat.api.persistance\parinaybharat.api.persistance.csproj" />
		<ProjectReference Include="..\parinaybharat.api.presentation\parinaybharat.api.presentation.csproj" />
	</ItemGroup>
```

### Application
```xml
	<ItemGroup>
		<!--MAPPING-->
		<PackageReference Include="AutoMapper" Version="13.0.1" />
		<!--VALIDATION-->
		<PackageReference Include="FluentValidation" Version="11.10.0" />
		<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.10.0" />
		<!--CQRS-->
		<PackageReference Include="MediatR" Version="12.4.1" />
		<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.10" />
	</ItemGroup>

	<ItemGroup>
	  <ProjectReference Include="..\parinaybharat.api.domain\parinaybharat.api.domain.csproj" />
	</ItemGroup>
```

### Persistance
```xml
	<ItemGroup>
		<!--EF CORE-->
		<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.10" />
		<PackageReference Include="MongoDB.EntityFrameworkCore" Version="8.1.1" />
	</ItemGroup>

	<ItemGroup>
	  <ProjectReference Include="..\parinaybharat.api.application\parinaybharat.api.application.csproj" />
	</ItemGroup>
```

### Presentation
```xml
	<ItemGroup>
		<!--OPEN API-->
		<PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
		<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.10" />
		<!--API VERSIONING-->
		<PackageReference Include="Asp.Versioning.Http" Version="8.1.0" />
		<!--MINIMAL API-->
		<PackageReference Include="Carter" Version="8.2.1" />
		<!--REDIS CACHE-->
		<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.10" />
	</ItemGroup>

	<ItemGroup>
	  <ProjectReference Include="..\parinaybharat.api.application\parinaybharat.api.application.csproj" />
	</ItemGroup>
```

# Program.cs

```cs
using Carter;
using Microsoft.OpenApi.Models;
using parinaybharat.api.Installers.Base;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

AutoInstall(builder.Host, builder.Services, builder.Configuration, Assembly.GetExecutingAssembly());

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

static void AutoInstall(IHostBuilder host, IServiceCollection services, IConfiguration configuration, Assembly assembly)
{
    var installers = assembly.GetTypes()
        .Where(t => typeof(IServiceInstaller).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        .Select(Activator.CreateInstance)
        .Cast<IServiceInstaller>()
        .ToList();

    installers.ForEach(installer => installer.InstallService(host, services, configuration));
}
```

# Installers

### ApiServiceInstaller
```cs
public sealed class ApiServiceInstaller : IServiceInstaller
{
    public void InstallService(IHostBuilder host, IServiceCollection services, IConfiguration configuration)
    {
        //Serilog + Seq
        var seqApiKey = configuration.GetConnectionString("Seq");
        host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore"))
                .Filter.ByExcluding(Matching.FromSource("Microsoft.Hosting"))
                .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Mvc"))
                .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore.Database.Command"))
                .WriteTo.Console(outputTemplate: "{Timestamp:dd/MM/yy hh:mm:ss tt} [{Level:u3}] {Message}{NewLine}{Exception}")
                .WriteTo.Async(x => x.Seq("https://seq.twileloop.com", apiKey: seqApiKey));
        });

        //CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
        });

        //JSON
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        //SWAGGER
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ParinayBharat API",
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }
}
```

### ApplicationServiceInstaller
```cs
    public sealed class ApplicationServiceInstaller : IServiceInstaller
    {
        public void InstallService(IHostBuilder host, IServiceCollection services, IConfiguration configuration)
        {
            //MediatR
            services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<IAppDBContext>());

            //Automapper
            services.AddAutoMapper(typeof(IAppDBContext));

            //Fluent Validator
            services.AddValidatorsFromAssemblyContaining<IAppDBContext>(ServiceLifetime.Singleton);
        }
    }
```

### PersistanceServiceInstaller
```cs
  public sealed class PersistanceServiceInstaller : IServiceInstaller
  {
      public void InstallService(IHostBuilder host, IServiceCollection services, IConfiguration configuration)
      {
          //EF Core
          var connectionString = configuration.GetConnectionString("TrackerDB");
          var dbName = "TrackerDB";
          services.AddDbContext<AppDBContext>(options => options.UseMongoDB(connectionString, dbName));
          services.AddScoped<IAppDBContext, AppDBContext>();
      }
  }
```

### PresentationServiceInstaller
```cs
public sealed class PresentationServiceInstaller : IServiceInstaller
{
    public void InstallService(IHostBuilder host, IServiceCollection services, IConfiguration configuration)
    {
        //Redis Cache
        var redisConnectionString = configuration.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "TrackerCache";
        });

        //Carter
        services.AddCarter();

        //API VERSIONING
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"));
        });
    }
}
```
