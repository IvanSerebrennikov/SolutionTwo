﻿using Microsoft.AspNetCore.Identity;
using SolutionTwo.Business.Common.PasswordHasher;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;

namespace SolutionTwo.Api.DI;

public static class BusinessCommonServicesRegistrationExtensions
{
    public static void AddBusinessCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();

        services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            options.IterationCount = 100_000;
        });

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
    }
}