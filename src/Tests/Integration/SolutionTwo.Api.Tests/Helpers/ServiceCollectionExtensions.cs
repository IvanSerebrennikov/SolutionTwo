using Microsoft.Extensions.DependencyInjection;

namespace SolutionTwo.Api.Tests.Helpers;

public static class ServiceCollectionExtensions
{
    public static void Replace<TInterface, TImplementation>(this IServiceCollection services)
    {
        var interfaceType = typeof(TInterface);
        var implementationType = typeof(TImplementation);
        var service = services.SingleOrDefault(x => x.ServiceType == interfaceType);
        if (service == null)
            return;
        
        var lifetime = service.Lifetime;
        services.Remove(service);

        services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
    }

    public static void Replace<TInterface>(this IServiceCollection services, Func<IServiceProvider, object> factory)
    {
        var interfaceType = typeof(TInterface);
        var service = services.SingleOrDefault(x => x.ServiceType == interfaceType);
        if (service == null)
            return;
        
        var lifetime = service.Lifetime;
        services.Remove(service);

        services.Add(new ServiceDescriptor(interfaceType, factory, lifetime));
    }
}