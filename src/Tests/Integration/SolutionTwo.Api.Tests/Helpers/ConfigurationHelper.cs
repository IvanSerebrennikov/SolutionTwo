using Microsoft.Extensions.Configuration;
using SolutionTwo.Api.Configuration;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Common.Configuration;

namespace SolutionTwo.Api.Tests.Helpers;

public static class ConfigurationHelper
{
    private static readonly Lazy<IConfigurationRoot> Configuration = new(() => new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.IntegrationTesting.json", optional: true)
        .AddEnvironmentVariables()
        .Build());

    private static ConnectionStrings? _connectionStrings;
    public static ConnectionStrings ConnectionStrings
    {
        get
        {
            return _connectionStrings ??= Configuration.Value.GetSection<ConnectionStrings>();
        }
    }
    
    private static BasicAuthenticationConfiguration? _basicAuthentication;
    public static BasicAuthenticationConfiguration BasicAuthentication
    {
        get
        {
            return _basicAuthentication ??= Configuration.Value.GetSection<BasicAuthenticationConfiguration>();
        }
    }
}