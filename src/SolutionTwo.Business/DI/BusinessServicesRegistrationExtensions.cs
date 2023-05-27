using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Business.Services;
using SolutionTwo.Business.Services.Interfaces;

namespace SolutionTwo.Business.DI;

public static class BusinessServicesRegistrationExtensions
{
    public static void AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
    }
}