using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Business.Configuration;
using SolutionTwo.Business.Services;
using SolutionTwo.Business.Services.Interfaces;
using SolutionTwo.Common.Extensions;

namespace SolutionTwo.Business.DI;

public static class BusinessServicesRegistrationExtensions
{
    public static void AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        var authConfiguration = configuration.GetSection<AuthConfiguration>();

        services.AddSingleton(authConfiguration);
        
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
    }
}