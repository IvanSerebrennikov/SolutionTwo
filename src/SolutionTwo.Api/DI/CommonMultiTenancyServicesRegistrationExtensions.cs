using SolutionTwo.Common.MultiTenancy;

namespace SolutionTwo.Api.DI;

public static class CommonMultiTenancyServicesRegistrationExtensions
{
    public static void AddCommonMultiTenancyServices(this IServiceCollection services)
    {
        services.AddScoped<TenantAccessProvider>();
        
        services.AddScoped<ITenantAccessGetter>(sp =>
            sp.GetService<TenantAccessProvider>() ??
            throw new InvalidOperationException($"{nameof(TenantAccessProvider)} isn't registered"));
        
        services.AddScoped<ITenantAccessSetter>(sp =>
            sp.GetService<TenantAccessProvider>() ??
            throw new InvalidOperationException($"{nameof(TenantAccessProvider)} isn't registered"));
    }
}