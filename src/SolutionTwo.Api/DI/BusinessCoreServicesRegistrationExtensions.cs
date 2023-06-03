using SolutionTwo.Business.Core.Services;
using SolutionTwo.Business.Core.Services.Interfaces;

namespace SolutionTwo.Api.DI;

public static class BusinessCoreServicesRegistrationExtensions
{
    public static void AddBusinessCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }
}