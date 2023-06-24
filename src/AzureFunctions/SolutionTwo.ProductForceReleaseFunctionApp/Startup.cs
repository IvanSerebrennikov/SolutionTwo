using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using SolutionTwo.ProductForceReleaseFunctionApp.DI;

[assembly: FunctionsStartup(typeof(SolutionTwo.ProductForceReleaseFunctionApp.Startup))]
namespace SolutionTwo.ProductForceReleaseFunctionApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;
        
        // Common DI
        builder.Services.AddCommonServices();
        
        // Data DI
        builder.Services.AddDataMainDatabaseServices(configuration);
    }
}