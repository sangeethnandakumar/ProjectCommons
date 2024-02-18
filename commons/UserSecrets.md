# User Secrets

### Initialise User-Secrets
```powershell
dotnet user-secrets init
```

This creates a user secret and link project into it
```xml
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <!-- ... -->

    <UserSecretsId>9eb92171-f2da-42de-bf68-e84425335b29</UserSecretsId>
</PropertyGroup>
```

Sample appsettings.json section
```json
{
    "Database": {
        "ConnectionString": {
            "DodoDB": "dev"
        }
    }
}
```

### List User-Secrets
```powershell
dotnet user-secrets list"
```

### Set User-Secrets
```powershell
dotnet user-secrets set "Database:ConnectionString:DodoDB" "Database=SomeDB.db;Username=someuser;Password=****"
```

### Remove User-Secrets
```powershell
dotnet user-secrets remove "Database:ConnectionString:DodoDB"
```