using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Common.Configuration;

namespace SolutionTwo.Data.Common.DI;

public static class DataCommonServicesRegistrationExtensions
{
    public static void AddDataCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration.GetSection<ConnectionStrings>();
        services.AddSingleton(connectionStrings);
    }
}