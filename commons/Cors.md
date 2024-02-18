# CORS Setup

### Full Open
```csharp
builder.Services.AddCors(options =>
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
app.UseCors("AllowAll");
```

### Specific Domains / IPs
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific",
        builder =>
        {
            builder
            .WithOrigins("http://google.com", "http://youtube.com")
            .WithHeaders("x-secure-origin")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .SetIsOriginAllowed(origin => new Uri(origin).Host.StartsWith("192.168.23."));
        });
});
app.UseCors("AllowSpecific");
```

### Specific Validatable Headers
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWithHeader",
        builder =>
        {
            builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .WithHeaders("x-secure-origin")
            .WithExposedHeaders("x-secure-origin")
            .SetIsOriginAllowed((host) => host.Headers["x-secure-origin"] == "apple");
        });
});
app.UseCors("AllowWithHeader");
```