using Microsoft.AspNetCore.Identity;
using SolutionTwo.Business.Common.PasswordHasher;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
using SolutionTwo.Business.Core.Services;
using SolutionTwo.Business.Core.Services.Interfaces;

namespace SolutionTwo.Api.DI;

public static class BusinessCoreServicesRegistrationExtensions
{
    public static void AddBusinessCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();

        services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            options.IterationCount = 100_000;
        });

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        
        services.AddScoped<IUserService, UserService>();
    }
}