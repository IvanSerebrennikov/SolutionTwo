using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Domain.Services;
using SolutionTwo.Domain.Services.Interfaces;

namespace SolutionTwo.Domain.DI;

public static class DomainServicesRegistrationExtensions
{
    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
    }
}