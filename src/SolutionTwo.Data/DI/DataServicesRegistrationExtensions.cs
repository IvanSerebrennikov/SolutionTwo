using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Configuration;
using SolutionTwo.Data.Context;
using SolutionTwo.Data.Repositories;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork;
using SolutionTwo.Data.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.DI;

public static class DataServicesRegistrationExtensions
{
    public static void AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration.GetSection<ConnectionStrings>();
        var databaseConfiguration = configuration.GetSection<DatabaseConfiguration>();

        services.AddSingleton(connectionStrings);
        services.AddSingleton(databaseConfiguration);

        services.AddDbContext<MainDatabaseContext>(o =>
            {
                o.UseSqlServer(connectionStrings.MainDatabaseConnection!,
                    op => op.CommandTimeout(databaseConfiguration.CommandTimeOutInSeconds));

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