using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Common.Configuration;
using SolutionTwo.Data.Common.Features.Audit;
using SolutionTwo.Data.Common.Features.MultiTenancy;
using SolutionTwo.Data.Common.Features.OptimisticConcurrency;
using SolutionTwo.Data.Common.Features.SoftDeletion;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Repositories;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.ProductForceReleaseFunctionApp.DI;

public static class DataServicesRegistrationExtensions
{
    public static void AddDataMainDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration.GetSection<ConnectionStrings>();
        
        services.AddSingleton(connectionStrings);
        
        services.AddScoped<ISoftDeletionContextBehavior, SoftDeletionContextBehavior>();
        
        services.AddScoped<IMultiTenancyContextBehavior, MultiTenancyContextBehavior>();
        
        services.AddScoped<IAuditContextBehavior, AuditContextBehavior>();
        
        services.AddScoped<IOptimisticConcurrencyContextBehavior, OptimisticConcurrencyContextBehavior>();
        
        services.AddDbContext<MainDatabaseContext>(o =>
            {
                o.UseSqlServer(connectionStrings.MainDatabaseConnection!);

                // Make sure that "Microsoft.EntityFrameworkCore" category is set to "None" 
                // for all production logging providers
                o.EnableSensitiveDataLogging();
            }
        );

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductUsageRepository, ProductUsageRepository>();
        
        services.AddScoped<IMainDatabase, MainDatabase>();
    }
}