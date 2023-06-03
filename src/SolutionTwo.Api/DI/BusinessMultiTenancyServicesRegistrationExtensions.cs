using SolutionTwo.Business.MultiTenancy.Services;
using SolutionTwo.Business.MultiTenancy.Services.Interfaces;

namespace SolutionTwo.Api.DI;

public static class BusinessMultiTenancyServicesRegistrationExtensions
{
    public static void AddBusinessMultiTenancyServices(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
    }
}