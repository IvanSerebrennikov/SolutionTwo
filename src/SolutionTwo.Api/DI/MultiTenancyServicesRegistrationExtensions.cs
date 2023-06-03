using SolutionTwo.MultiTenancy;

namespace SolutionTwo.Api.DI;

public static class MultiTenancyServicesRegistrationExtensions
{
    public static void AddMultiTenancyServices(this IServiceCollection services)
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