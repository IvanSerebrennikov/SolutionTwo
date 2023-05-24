using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Identity.PasswordManaging;
using SolutionTwo.Identity.PasswordManaging.Interfaces;
using SolutionTwo.Identity.TokenManaging;
using SolutionTwo.Identity.TokenManaging.Interfaces;

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