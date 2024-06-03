## Solution Structuring

![image](https://github.com/sangeethnandakumar/ProjectCommons/assets/24974154/721f0d12-9a71-44f8-b771-ea5554a87c1f)

# A. Domain

#### Packages
```xml
  <ItemGroup>
    <PackageReference Include="LanguageExt.Core" Version="4.4.8" />
  </ItemGroup>
```

#### Primitives/Entity.cs
```csharp
namespace Domain.Primitives
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime? CreatedOn { get; protected set; }
        public DateTime? UpdatedOn { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTime.UtcNow;
        }

        protected Entity(Guid id)
        {
            Id = id;
            CreatedOn = DateTime.UtcNow;
        }
    }
}
```

#### Entities/User.cs
```csharp
using Domain.Enums;
using Domain.Primitives;

namespace Domain.Users
{
    public sealed class User : Entity
    {
        public User(string firstName, string? lastName, string username, DateTime? dateOfBirth, LoginMethod loginMethod, Gender? gender)
        {
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            HashedPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            DateOfBirth = dateOfBirth;
            LoginMethod = loginMethod;
            Gender = gender;
        }

        public string FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string Username { get; private set; }
        public string HashedPassword { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public LoginMethod LoginMethod { get; private set; }
        public Gender? Gender { get; private set; }
    }
}
```

### Exceptions/Base/LoginException.cs
```csharp
namespace Domain.Exceptions.Base
{
    public class LoginException : Exception
    {
        public LoginException(string message) : base(message)
        {

        }
    }
}
```

### Exceptions/InvalidLoginException.cs
```csharp
using Domain.Exceptions.Base;

namespace Domain.Exceptions
{
    public sealed class InvalidLoginException : LoginException
    {
        public InvalidLoginException(string username) : base($"Invalid username: {username}")
        {
        }
    }
}
```

### Enums/Gender.cs
```csharp
namespace Domain.Enums
{
    public enum Gender
    {
        MALE,
        FEMALE,
        OTHERS
    }
}
```

### Abstractions/IUserRepository.cs
```csharp
using Domain.Users;
using LanguageExt.Common;

namespace Domain.Abstractions
{
    public interface IUserRepository
    {
        Result<Guid> CreateUser(User user);
        Result<Guid> UpdateUser(User user);
        Result<Guid> DeleteUser(User user);
    }
}
```

# B. Application

### Packages
```xml
  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ExpenceTracker.Domain\Domain.csproj" />
  </ItemGroup>
```

### Users/Commands/CreateUser/CreateUserCommand.cs
```csharp
using Domain.Enums;
using LanguageExt.Common;
using MediatR;

namespace Application.Users.Commands.CreateUser
{
    public record CreateUserCommand(
        string FirstName,
        string? LastName,
        string Username,
        DateTime? DateOfBirth,
        LoginMethod LoginMethod,
        Gender? Gender)
        : IRequest<Result<Guid>>;
}
```

### Users/Commands/CreateUser/CreateUserCommandHandler.cs
```csharp
using Domain.Abstractions;
using Domain.Users;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
    {
        private readonly ILogger<CreateUserCommandHandler> logger;
        private readonly IUserRepository userRepo;

        public CreateUserCommandHandler(ILogger<CreateUserCommandHandler> logger, IUserRepository userRepo)
        {
            this.logger = logger;
            this.userRepo = userRepo;
        }

        public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            //Create user
            var user = new User(
                request.FirstName,
                request.LastName,
                request.Username,
                request.DateOfBirth,
                request.LoginMethod,
                request.Gender
                );

            var createUserResult = userRepo.CreateUser(user);

            if (createUserResult.IsFaulted)
            {
                logger.LogDebug($"Unable to create a new user {createUserResult}");
                return createUserResult;
            }

            return createUserResult;
        }
    }
}
```

# C. Infrastructure

### Packages
```xml
  <ItemGroup>
    <PackageReference Include="LanguageExt.Core" Version="4.4.8" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ExpenceTracker.Domain\Domain.csproj" />
  </ItemGroup>
```

### Repositories/UserRepository.cs
```csharp
using Domain.Abstractions;
using Domain.Exceptions;
using Domain.Users;
using LanguageExt.Common;

namespace Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        public Result<Guid> CreateUser(User user)
        {
            return new Result<Guid>(new InvalidLoginException(user.Username));
        }

        public Result<Guid> DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public Result<Guid> UpdateUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
```

# D. Presentation

### Packages
```xml
  <ItemGroup>
  	<PackageReference Include="MediatR" Version="12.2.0" />
  	<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.6" />
    <PackageReference Include="Carter" Version="8.2.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ExpenceTracker.Application\Application.csproj" />
  </ItemGroup>
```

### User/UserModule.cs
```csharp
### User/UserModule.cs
```csharp
using Application.Users.Commands.CreateUser;
using Carter;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Presentation.User
{
    public sealed class UserModule : CarterModule
    {
        public UserModule() : base("user")
        {
            WithTags("User Endpoints");
        }

        public override void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new CreateUserCommand(
                    "Sangeeth",
                    "Nandakumar",
                    "sangee",
                    DateTime.Now,
                    LoginMethod.PASSWORD,
                    Gender.MALE));

                return result.Match(s => Results.Ok(), f => Results.BadRequest(f.Message));
            })
            .WithSummary("Logs Income/Expence")
            .WithDescription("Adds income or expence from the user");
        }
    }

}
```

# Main API Project (Expence Tracker)

### Packages
```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.6" />
    <PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" Version="1.20.1" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ExpenceTracker.Infrastructure\Infrastructure.csproj" />
    <ProjectReference Include="..\ExpenceTracker.Presentation\Presentation.csproj" />
  </ItemGroup>
```

### Installers/IServiceInstaller.cs
```csharp
namespace ExpenceTracker.Installers
{
    public interface IServiceInstaller
    {
        void InstallService(IServiceCollection services, IConfiguration configuration);
    }
}
```

### Installers/ApplicationServiceInstaller.cs
```csharp
using Application.Users.Commands.CreateUser;
using Domain.Abstractions;
using Infrastructure.Repositories;

namespace ExpenceTracker.Installers
{
    public sealed class ApplicationServiceInstaller : IServiceInstaller
    {
        public void InstallService(IServiceCollection services, IConfiguration configuration)
        {
            //MediatR
            services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<CreateUserCommand>());

            //Dependencies
            services.AddSingleton<IUserRepository, UserRepository>();
        }
    }
}
```

### Installers/InfrastructureServiceInstaller.cs
```csharp
namespace ExpenceTracker.Installers
{
    public sealed class InfrastructureServiceInstaller : IServiceInstaller
    {
        public void InstallService(IServiceCollection services, IConfiguration configuration)
        {

        }
    }
}
```
