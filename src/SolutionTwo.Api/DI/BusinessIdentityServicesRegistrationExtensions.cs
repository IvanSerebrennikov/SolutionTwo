using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;
using SolutionTwo.Business.Identity.TokenStore;
using SolutionTwo.Business.Identity.TokenStore.Interfaces;

namespace SolutionTwo.Api.DI;

public static class BusinessIdentityServicesRegistrationExtensions
{
    public static void AddBusinessIdentityServices(this IServiceCollection services)
    {
        services.AddSingleton<ITokenProvider, JwtProvider>();
        
        services.AddSingleton<IRevokedTokenStore, RevokedTokenMemoryCacheStore>();

        services.AddScoped<IIdentityService, IdentityService>();
    }
}