using System;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Common.LoggedInUserAccessor;
using SolutionTwo.Common.LoggedInUserAccessor.Interfaces;
using SolutionTwo.Common.MaintenanceStatusAccessor;
using SolutionTwo.Common.MaintenanceStatusAccessor.Interfaces;
using SolutionTwo.Common.TenantAccessor;
using SolutionTwo.Common.TenantAccessor.Interfaces;

namespace SolutionTwo.FunctionApp.DataBaseOperations.DI;

public static class CommonServicesRegistrationExtensions
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        services.AddScoped<TenantAccessor>();
        
        services.AddScoped<ITenantAccessGetter>(sp =>
            sp.GetService<TenantAccessor>() ??
            throw new InvalidOperationException($"{nameof(TenantAccessor)} isn't registered"));
        
        services.AddScoped<ITenantAccessSetter>(sp =>
            sp.GetService<TenantAccessor>() ??
            throw new InvalidOperationException($"{nameof(TenantAccessor)} isn't registered"));
        
        services.AddScoped<LoggedInUserAccessor>();
        
        services.AddScoped<ILoggedInUserGetter>(sp =>
            sp.GetService<LoggedInUserAccessor>() ??
            throw new InvalidOperationException($"{nameof(LoggedInUserAccessor)} isn't registered"));
        
        services.AddScoped<ILoggedInUserSetter>(sp =>
            sp.GetService<LoggedInUserAccessor>() ??
            throw new InvalidOperationException($"{nameof(LoggedInUserAccessor)} isn't registered"));
        
        services.AddSingleton<MaintenanceStatusAccessor>();
        
        services.AddSingleton<IMaintenanceStatusGetter>(sp =>
            sp.GetService<MaintenanceStatusAccessor>() ??
            throw new InvalidOperationException($"{nameof(MaintenanceStatusAccessor)} isn't registered"));
        
        services.AddSingleton<IMaintenanceStatusSetter>(sp =>
            sp.GetService<MaintenanceStatusAccessor>() ??
            throw new InvalidOperationException($"{nameof(MaintenanceStatusAccessor)} isn't registered"));
    }
}