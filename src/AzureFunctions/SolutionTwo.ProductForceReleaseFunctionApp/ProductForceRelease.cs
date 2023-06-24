using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace SolutionTwo.ProductForceReleaseFunctionApp;

public static class ProductForceRelease
{
    [FunctionName("ProductForceRelease")]
    public static async Task RunAsync([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
        
    }
}