using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Common.Configuration;
using SolutionTwo.Data.MainDatabase.Configuration;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Repositories;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.MainDatabase.DI;

public static class DataMainDatabaseServicesRegistrationExtensions
{
    public static void AddDataMainDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration.GetSection<ConnectionStrings>();
        var databaseConfiguration = configuration.GetSection<MainDatabaseConfiguration>();
        
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
        services.AddScoped<IMainDatabase, UnitOfWork.MainDatabase>();
    }
}