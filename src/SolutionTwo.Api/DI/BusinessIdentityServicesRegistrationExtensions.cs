using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenManager;
using SolutionTwo.Business.Identity.TokenManager.Interfaces;

namespace SolutionTwo.Api.DI;

public static class BusinessIdentityServicesRegistrationExtensions
{
    public static void AddBusinessIdentityServices(this IServiceCollection services)
    {
        services.AddSingleton<ITokenManager, JwtManager>();

        services.AddScoped<IAuthService, AuthService>();
    }
}