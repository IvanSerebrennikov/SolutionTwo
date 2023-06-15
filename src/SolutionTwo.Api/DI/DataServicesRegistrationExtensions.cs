using Microsoft.EntityFrameworkCore;
using SolutionTwo.Api.Extensions;
using SolutionTwo.Data.Common.Configuration;
using SolutionTwo.Data.MainDatabase.Configuration;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Repositories;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Api.DI;

public static class DataServicesRegistrationExtensions
{
    public static void AddDataMainDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration.GetSection<ConnectionStrings>();
        var mainDatabaseConfiguration = configuration.GetSection<MainDatabaseConfiguration>();
        
        services.AddSingleton(connectionStrings);
        
        services.AddSingleton(mainDatabaseConfiguration);
        
        services.AddDbContext<MainDatabaseContext>(o =>
            {
                o.UseSqlServer(connectionStrings.MainDatabaseConnection!,
                    x => x.CommandTimeout(mainDatabaseConfiguration.CommandTimeOutInSeconds));

                // Make sure that "Microsoft.EntityFrameworkCore" category is set to "None" 
                // for all production logging providers
                o.EnableSensitiveDataLogging();

                // 'Update' repository method should be called to mark entity/properties as changed
                o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        );

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IMainDatabase, MainDatabase>();
    }
}