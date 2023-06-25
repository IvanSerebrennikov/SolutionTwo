using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using SolutionTwo.FunctionApp.DataBaseOperations.DI;

[assembly: FunctionsStartup(typeof(SolutionTwo.FunctionApp.DataBaseOperations.Startup))]
namespace SolutionTwo.FunctionApp.DataBaseOperations;

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