using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Identity.Configuration;
using SolutionTwo.Identity.PasswordProcessing;
using SolutionTwo.Identity.PasswordProcessing.Interfaces;
using SolutionTwo.Identity.TokenProvider;
using SolutionTwo.Identity.TokenProvider.Interfaces;

namespace SolutionTwo.Identity.DI;

public static class IdentityServicesRegistrationExtension
{
    public static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher<object>, PasswordHasher<object>>();
        
        services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            options.IterationCount = 100_000;
        });
        
        services.AddScoped<IPasswordProcessor, PasswordProcessor>();
        
        services.AddScoped<ITokenProvider, JwtProvider>();
    }
}