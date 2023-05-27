using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Business.Core.Services;
using SolutionTwo.Business.Core.Services.Interfaces;

namespace SolutionTwo.Business.Core.DI;

public static class BusinessCoreServicesRegistrationExtensions
{
    public static void AddBusinessCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserService, UserService>();
    }
}