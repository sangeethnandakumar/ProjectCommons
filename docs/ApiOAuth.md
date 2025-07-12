# Login WIth Google/Apple via `/auth/oauth` .NET endpoint

<img width="1499" height="866" alt="image" src="https://github.com/user-attachments/assets/394a6b3e-d2d6-4bca-8b59-cfa60a0f758e" />

## Google's Public Key

> https://www.googleapis.com/oauth2/v3/certs

## Apple's Public Key

> https://appleid.apple.com/auth/keys

## Google Validate

```csharp
static async Task<JwtSecurityToken?> ValidateGoogleIdToken(string idToken)
{
    var googleIssuer = "https://accounts.google.com";
    var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
        $"{googleIssuer}/.well-known/openid-configuration",
        new OpenIdConnectConfigurationRetriever()
    );

    var config = await configManager.GetConfigurationAsync();
    var validationParams = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = googleIssuer,
        ValidateAudience = true,
        ValidAudience = "<your-client-id>",
        ValidateLifetime = true,
        IssuerSigningKeys = config.SigningKeys
    };

    var handler = new JwtSecurityTokenHandler();
    try
    {
        handler.ValidateToken(idToken, validationParams, out var validatedToken);
        return validatedToken as JwtSecurityToken;
    }
    catch
    {
        return null;
    }
}
```

## Apple Validate

```csharp
static async Task<JwtSecurityToken?> ValidateAppleIdToken(string idToken)
{
    var appleIssuer = "https://appleid.apple.com";
    var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
        $"{appleIssuer}/.well-known/openid-configuration",
        new OpenIdConnectConfigurationRetriever()
    );

    var config = await configManager.GetConfigurationAsync();
    var validationParams = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = appleIssuer,
        ValidateAudience = true,
        ValidAudience = "<your-client-id>",
        ValidateLifetime = true,
        IssuerSigningKeys = config.SigningKeys
    };

    var handler = new JwtSecurityTokenHandler();
    try
    {
        handler.ValidateToken(idToken, validationParams, out var validatedToken);
        return validatedToken as JwtSecurityToken;
    }
    catch
    {
        return null;
    }
}
```

## Issue New JWT Token

```csharp
static string GenerateAppJwt(string userId, string? email)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, email ?? "")
    };

    var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("your-very-secret-key"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "your-app",
        audience: "your-app",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```
