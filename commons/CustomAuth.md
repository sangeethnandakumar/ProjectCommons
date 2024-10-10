# Complete Custom Authentication & Authorization Handlers

## Authentication Handler
```cs
public class ForgeRockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<ForgeRockAuthenticationHandler> logger;
    private readonly IOptions<ForgeRockOptions> forgeRockOptions;

    public ForgeRockAuthenticationHandler(IOptions<ForgeRockOptions> forgeRockOptions, IHttpContextAccessor httpContextAccessor = null) 
        : base(options, loggerFactory, encoder, clock)
    {
        this.logger = logger;
        this.forgeRockOptions = forgeRockOptions;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        logger.LogInformation("Inside Authentication Handler");
        var claims = new List<Claim>();
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

## Authorization Handler
```cs
public class ForgeRockAuthorizationHandler : AuthorizationHandler<ForgeRockAuthorizationRequirement>
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IOptions<ForgeRockOptions> forgeRockOptions;
    private readonly ILogger<ForgeRockAuthorizationHandler> logger;

    public ForgeRockAuthorizationHandler(IHttpClientFactory httpClientFactory, IOptions<ForgeRockOptions> forgeRockOptions, ILogger<ForgeRockAuthorizationHandler> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.forgeRockOptions = forgeRockOptions;
        this.logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ForgeRockAuthorizationRequirement requirement)
    {
        logger.LogInformation("Inside Authorization Handler");

        if (!context.User.Identity.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```
