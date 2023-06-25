using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SolutionTwo.Common.TenantAccessor.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.FunctionApp.DataBaseOperations.Functions;

public class ProductsForceReleaseFunction
{
    private readonly IMainDatabase _mainDatabase;
    private readonly DateTime _minUsageStartDateTimeUtc;
    
    public ProductsForceReleaseFunction(ITenantAccessSetter tenantAccessSetter, IMainDatabase mainDatabase)
    {
        tenantAccessSetter.SetAccessToAllTenants();
        _mainDatabase = mainDatabase;
        _minUsageStartDateTimeUtc = DateTime.UtcNow.AddMinutes(-20);
    }

    // every minute
    [FunctionName("ProductsForceReleaseFunction")]
    public async Task RunAsync([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation($"C# ProductsForceRelease Timer trigger function execution started at: {DateTime.UtcNow}");

        try
        {
            await foreach (var releasedProductId in ReleaseProductsAsync())
            {
                if (releasedProductId != null)
                {
                    log.LogInformation("Released product: {id}", releasedProductId);
                }
            }
        }
        catch (Exception e)
        {
            log.LogError(e, "Exception occured during products releasing");
        }

        log.LogInformation($"C# ProductsForceRelease Timer trigger function execution finished at: {DateTime.UtcNow}");
    }

    private async IAsyncEnumerable<Guid?> ReleaseProductsAsync()
    {
        var productsToReleasePreliminaryList = await _mainDatabase.Products.GetAsync(x =>
            x.ProductUsages.Any(u =>
                u.ReleasedDateTimeUtc == null && 
                u.UsageStartDateTimeUtc < _minUsageStartDateTimeUtc));

        foreach (var productToRelease in productsToReleasePreliminaryList)
        {
            yield return await ReleaseProductAsync(productToRelease.Id);
        }
    }

    private async Task<Guid?> ReleaseProductAsync(Guid productId)
    {
        var result = await _mainDatabase.ExecuteInTransactionWithRetryAsync<Guid?>(async () =>
            {
                var productEntity = await _mainDatabase.Products.GetByIdAsync(productId,
                    include: x =>
                        x.ProductUsages.Where(u =>
                            u.ReleasedDateTimeUtc == null &&
                            u.UsageStartDateTimeUtc < _minUsageStartDateTimeUtc));
                
                if (productEntity == null)
                {
                    return null;
                }

                if (productEntity.ProductUsages.Count == 0)
                {
                    return null;
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
                
                return productEntity.Id;
            },
            // default isolation level ReadCommitted is enough here because of Optimistic Concurrency
            delayBetweenRetries: TimeSpan.FromMilliseconds(300));

        return result;
    }
}