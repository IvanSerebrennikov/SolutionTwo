using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Data.Repositories;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork;
using SolutionTwo.Data.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.DI;

public static class DataServicesRegistrationExtensions
{
    public static void AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IMainDatabase, MainDatabase>();
    }
}