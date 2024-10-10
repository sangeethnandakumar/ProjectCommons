# Complete Custom Authentication & Authorization Handlers

## Authentication Handler

```csharp
public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CustomAuthenticationHandler> _logger;
    private readonly IOptions<CustomOptions> _customOptions;

    public CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock,
        IOptions<CustomOptions> customOptions,
        IHttpContextAccessor httpContextAccessor = null)
        : base(options, loggerFactory, encoder, clock)
    {
        _logger = loggerFactory.CreateLogger<CustomAuthenticationHandler>();
        _customOptions = customOptions;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        _logger.LogInformation("Inside Authentication Handler");
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

## Authorization Handler

```csharp
public class CustomAuthorizationHandler : AuthorizationHandler<CustomAuthorizationRequirement>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<CustomOptions> _customOptions;
    private readonly ILogger<CustomAuthorizationHandler> _logger;

    public CustomAuthorizationHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<CustomOptions> customOptions,
        ILogger<CustomAuthorizationHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _customOptions = customOptions;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomAuthorizationRequirement requirement)
    {
        _logger.LogInformation("Inside Authorization Handler");
        if (!context.User.Identity.IsAuthenticated)
        {
            return Task.CompletedTask;
        }
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```

## Authorization Requirement

```csharp
public class CustomAuthorizationRequirement : IAuthorizationRequirement { }
```

## Options

```csharp
public class CustomOptions
{
    public string Endpoint { get; set; }
}
```

---

## DI Wiring In Program.cs

```csharp
//...
builder.Services.AddCustomAuthorization();
builder.Services.AddCustomAuthentication();
//...
app.UseAuthentication();
app.UseAuthorization();
//...

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.Configure<CustomOptions>(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("Custom"));
        services.AddHttpClient();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("CustomPolicy", policy =>
            {
                policy.AuthenticationSchemes.Add("Custom");
                policy.Requirements.Add(new CustomAuthorizationRequirement());
            });
        });
        services.AddSingleton<IAuthorizationHandler, CustomAuthorizationHandler>();
        services.AddSingleton<IAuthorizationRequirement, CustomAuthorizationRequirement>();
        return services;
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication("Custom")
            .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("Custom", null);
        return services;
    }
}
```
