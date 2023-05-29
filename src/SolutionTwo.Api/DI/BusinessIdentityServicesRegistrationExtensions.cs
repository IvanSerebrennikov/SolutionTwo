using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;

namespace SolutionTwo.Api.DI;

public static class BusinessIdentityServicesRegistrationExtensions
{
    public static void AddBusinessIdentityServices(this IServiceCollection services)
    {
        services.AddSingleton<ITokenProvider, JwtProvider>();

        services.AddScoped<IAuthService, AuthService>();
    }
}