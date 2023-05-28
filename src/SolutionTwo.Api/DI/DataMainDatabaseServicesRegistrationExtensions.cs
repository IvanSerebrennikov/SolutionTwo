﻿using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Configuration;
using SolutionTwo.Data.MainDatabase.Configuration;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Repositories;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Api.DI;

public static class DataMainDatabaseServicesRegistrationExtensions
{
    public static void AddDataMainDatabaseServices(this IServiceCollection services,
        ConnectionStrings connectionStrings, MainDatabaseConfiguration databaseConfiguration)
    {
        services.AddSingleton(databaseConfiguration);

        services.AddDbContext<MainDatabaseContext>(o =>
            {
                o.UseSqlServer(connectionStrings.MainDatabaseConnection!,
                    x => x.CommandTimeout(databaseConfiguration.CommandTimeOutInSeconds));

                // Make sure that "Microsoft.EntityFrameworkCore" category is set to "None" 
                // for all providers except "Debug"
                o.EnableSensitiveDataLogging();
            }
        );

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IMainDatabase, MainDatabase>();
    }
}