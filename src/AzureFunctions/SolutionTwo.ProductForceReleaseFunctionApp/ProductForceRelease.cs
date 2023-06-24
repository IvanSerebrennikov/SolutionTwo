using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SolutionTwo.Common.TenantAccessor.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.ProductForceReleaseFunctionApp;

public class ProductForceRelease
{
    private readonly IMainDatabase _mainDatabase;
    private readonly DateTime _minUsageStartDateTimeUtc;
    
    public ProductForceRelease(ITenantAccessSetter tenantAccessSetter, IMainDatabase mainDatabase)
    {
        tenantAccessSetter.SetAccessToAllTenants();
        _mainDatabase = mainDatabase;
        _minUsageStartDateTimeUtc = DateTime.UtcNow.AddMinutes(-20);
    }

    [FunctionName("ProductForceRelease")]
    public async Task RunAsync([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation($"C# ProductForceRelease Timer trigger function execution started at: {DateTime.UtcNow}");

        try
        {
            var releasedProductsCount = await ReleaseProductsAsync();
            
            log.LogInformation("Released products count: {count}", releasedProductsCount);
        }
        catch (Exception e)
        {
            log.LogError(e, "Exception occured during products releasing");
        }

        log.LogInformation($"C# ProductForceRelease Timer trigger function execution finished at: {DateTime.UtcNow}");
    }

    private async Task<int> ReleaseProductsAsync()
    {
        var productsToReleasePreliminaryList = await _mainDatabase.Products.GetAsync(x =>
            x.ProductUsages.Any(u =>
                u.ReleasedDateTimeUtc == null && u.UsageStartDateTimeUtc < _minUsageStartDateTimeUtc));

        var releasedProductsCount = 0;
        
        foreach (var productToRelease in productsToReleasePreliminaryList)
        {
            var released = await ReleaseProductAsync(productToRelease.Id);

            if (released)
                releasedProductsCount++;
        }

        return releasedProductsCount;
    }

    private async Task<bool> ReleaseProductAsync(Guid productId)
    {
        var result = await _mainDatabase.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var productEntity = await _mainDatabase.Products.GetByIdAsync(productId,
                    include: x =>
                        x.ProductUsages.Where(u =>
                            u.ReleasedDateTimeUtc == null &&
                            u.UsageStartDateTimeUtc < _minUsageStartDateTimeUtc));
                
                if (productEntity == null)
                {
                    return false;
                }

                if (productEntity.ProductUsages.Count == 0)
                {
                    return false;
                }

                foreach (var usageEntity in productEntity.ProductUsages)
                {
                    usageEntity!.ReleasedDateTimeUtc = DateTime.UtcNow;
                    usageEntity!.IsForceReleased = true;
                    _mainDatabase.ProductUsages.Update(usageEntity, x => x.ReleasedDateTimeUtc, x => x.IsForceReleased);
                    
                    productEntity.CurrentActiveUsagesCount--;
                }
                
                _mainDatabase.Products.Update(productEntity, x => x.CurrentActiveUsagesCount);
                
                await _mainDatabase.CommitChangesAsync();
                
                return true;
            },
            // default isolation level ReadCommitted is enough here because of Optimistic Concurrency
            delayBetweenRetries: TimeSpan.FromMilliseconds(300));

        return result;
    }
}