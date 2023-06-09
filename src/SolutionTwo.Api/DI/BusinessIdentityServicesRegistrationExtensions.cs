﻿using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;
using SolutionTwo.Business.Identity.TokenStore;
using SolutionTwo.Business.Identity.TokenStore.Interfaces;
using SolutionTwo.Common.Extensions;

namespace SolutionTwo.Api.DI;

public static class BusinessIdentityServicesRegistrationExtensions
{
    public static void AddBusinessIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        var identityConfiguration = configuration.GetSection<IdentityConfiguration>();
        
        services.AddSingleton(identityConfiguration);

        services.AddSingleton<ITokenProvider, JwtProvider>();
        
        services.AddSingleton<IDeactivatedTokenStore, DeactivatedTokenMemoryCacheStore>();

        services.AddScoped<IIdentityService, IdentityService>();
    }
}