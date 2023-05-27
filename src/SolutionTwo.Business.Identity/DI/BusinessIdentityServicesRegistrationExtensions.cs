using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Business.Common.PasswordManager;
using SolutionTwo.Business.Common.PasswordManager.Interfaces;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenManager;
using SolutionTwo.Business.Identity.TokenManager.Interfaces;
using SolutionTwo.Common.Extensions;

namespace SolutionTwo.Business.Identity.DI;

public static class BusinessIdentityServicesRegistrationExtensions
{
    public static void AddBusinessIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfiguration = configuration.GetSection<IdentityConfiguration>();

        services.AddSingleton(jwtConfiguration);
        
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();

        services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            options.IterationCount = 100_000;
        });

        services.AddSingleton<IPasswordManager, PasswordManager>();

        services.AddSingleton<ITokenManager, JwtManager>();
        
        services.AddScoped<IAuthService, AuthService>();
    }
}