using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Identity.PasswordManagement;
using SolutionTwo.Identity.PasswordManagement.Interfaces;
using SolutionTwo.Identity.TokenManagement;
using SolutionTwo.Identity.TokenManagement.Interfaces;

namespace SolutionTwo.Identity.DI;

public static class IdentityServicesRegistrationExtensions
{
    public static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();

        services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            options.IterationCount = 100_000;
        });

        services.AddSingleton<IPasswordManager, PasswordManager>();

        services.AddSingleton<ITokenManager, JwtManager>();
    }
}