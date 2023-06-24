using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Core.Models.Product.Incoming;
using SolutionTwo.Business.Core.Models.Product.Outgoing;
using SolutionTwo.Business.Core.Services.Interfaces;
using SolutionTwo.Common.LoggedInUserAccessor.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Core.Services;

public class ProductService : IProductService
{
    private readonly IMainDatabase _mainDatabase;
    private readonly ILoggedInUserGetter _loggedInUserGetter;
    private readonly ILogger<UserService> _logger;

    public ProductService(
        IMainDatabase mainDatabase, 
        ILogger<UserService> logger, 
        ILoggedInUserGetter loggedInUserGetter)
    {
        _mainDatabase = mainDatabase;
        _logger = logger;
        _loggedInUserGetter = loggedInUserGetter;
    }

    public async Task<IServiceResult> UseProductAsync(Guid id)
    {
        var result = await _mainDatabase.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var loggedInUserId = _loggedInUserGetter.UserId;

                if (loggedInUserId == null)
                {
                    throw new ApplicationException(
                        "Logged In User info isn't properly configured");
                }
                
                var productEntity = await _mainDatabase.Products.GetByIdAsync(id,
                    include: x => x.ProductUsages.Where(u => u.ReleasedDateTimeUtc == null));
                
                if (productEntity == null)
                {
                    return ServiceResult.Error(
                        "Product was not found");
                }

                if (productEntity.MaxActiveUsagesCount == productEntity.CurrentActiveUsagesCount)
                {
                    return ServiceResult.Error(
                        $"Product has maximum number of current active usages " +
                        $"({productEntity.MaxActiveUsagesCount})");
                }

                if (productEntity.ProductUsages.Any(x => x.UserId == loggedInUserId))
                {
                    return ServiceResult.Error(
                        "Product already has active usage for authorized User");
                }

                var newUsageEntity = new ProductUsageEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = productEntity.Id,
                    UserId = loggedInUserId.Value,
                    UsageStartDateTimeUtc = DateTime.UtcNow
                };
                
                _mainDatabase.ProductUsages.Create(newUsageEntity);
                
                productEntity.CurrentActiveUsagesCount++;
                _mainDatabase.Products.Update(productEntity, x => x.CurrentActiveUsagesCount);
                
                await _mainDatabase.CommitChangesAsync();
                
                return ServiceResult.Success();
            },
            // default isolation level ReadCommitted is enough here because of Optimistic Concurrency
            delayBetweenRetries: TimeSpan.FromMilliseconds(300));

        return result ?? ServiceResult.Error("Error occured during DB transaction executing");
    }

    public async Task<IServiceResult> ReleaseProductAsync(Guid id)
    {
        var result = await _mainDatabase.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var loggedInUserId = _loggedInUserGetter.UserId;

                if (loggedInUserId == null)
                {
                    throw new ApplicationException(
                        "Logged In User info isn't properly configured");
                }

                var productEntity = await _mainDatabase.Products.GetByIdAsync(id,
                    include: x =>
                        x.ProductUsages.Where(u => u.ReleasedDateTimeUtc == null && u.UserId == loggedInUserId));
                
                if (productEntity == null)
                {
                    return ServiceResult.Error(
                        "Product was not found");
                }

                if (productEntity.ProductUsages.Count == 0)
                {
                    return ServiceResult.Error(
                        "Product hasn't active usages for authorized User");
                }
                
                if (productEntity.ProductUsages.Count > 1)
                {
                    throw new ApplicationException(
                        "Product can't have more than 1 active usage per User");
                }

                var usageEntity = productEntity.ProductUsages.SingleOrDefault();
                
                usageEntity!.ReleasedDateTimeUtc = DateTime.UtcNow;
                _mainDatabase.ProductUsages.Update(usageEntity, x => x.ReleasedDateTimeUtc);

                productEntity.CurrentActiveUsagesCount--;
                _mainDatabase.Products.Update(productEntity, x => x.CurrentActiveUsagesCount);
                
                await _mainDatabase.CommitChangesAsync();
                
                return ServiceResult.Success();
            },
            // default isolation level ReadCommitted is enough here because of Optimistic Concurrency
            delayBetweenRetries: TimeSpan.FromMilliseconds(300));

        return result ?? ServiceResult.Error("Error occured during DB transaction executing");
    }

    public async Task<ProductWithActiveUsagesModel?> GetProductWithActiveUsagesByIdAsync(Guid id)
    {
        var productEntity = await _mainDatabase.Products.GetByIdAsync(id,
            include: x => x.ProductUsages.Where(u => u.ReleasedDateTimeUtc == null));

        return productEntity != null ? new ProductWithActiveUsagesModel(productEntity) : null;
    }

    public async Task<IReadOnlyList<ProductWithActiveUsagesModel>> GetAllProductsWithActiveUsagesAsync()
    {
        var productEntities = await _mainDatabase.Products.GetAsync(
            include: x => x.ProductUsages.Where(u => u.ReleasedDateTimeUtc == null));
        var productModels = productEntities.Select(x => new ProductWithActiveUsagesModel(x)).ToList();

        return productModels;
    }

    public async Task<IServiceResult<ProductWithActiveUsagesModel>> CreateProductAsync(
        CreateProductModel createProductModel)
    {
        var existingProductWithSameName =
            await _mainDatabase.Products.GetSingleAsync(x => x.Name == createProductModel.Name);
        if (existingProductWithSameName != null)
        {
            return ServiceResult<ProductWithActiveUsagesModel>.Error(
                $"Product with name '{createProductModel.Name}' already exists");
        }

        var productEntity = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = createProductModel.Name,
            MaxActiveUsagesCount = createProductModel.MaxActiveUsagesCount
        };

        _mainDatabase.Products.Create(productEntity);

        await _mainDatabase.CommitChangesAsync();

        var model = new ProductWithActiveUsagesModel(productEntity);

        return ServiceResult<ProductWithActiveUsagesModel>.Success(model);
    }

    public async Task<IServiceResult> UpdateProductAsync(
        UpdateProductModel updateProductModel)
    {
        var result = await _mainDatabase.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var productEntity = await _mainDatabase.Products.GetByIdAsync(updateProductModel.Id);
                if (productEntity == null)
                {
                    return ServiceResult.Error(
                        "Product was not found");
                }

                if (updateProductModel.MaxActiveUsagesCount < productEntity.CurrentActiveUsagesCount)
                {
                    return ServiceResult.Error(
                        "Product has more current active usages " +
                        "than provided in new value for MaxActiveUsagesCount");
                }
                
                productEntity.Name = updateProductModel.Name;
                _mainDatabase.Products.Update(productEntity, x => x.Name);

                if (productEntity.MaxActiveUsagesCount != updateProductModel.MaxActiveUsagesCount)
                {
                    productEntity.MaxActiveUsagesCount = updateProductModel.MaxActiveUsagesCount;
                    _mainDatabase.Products.Update(productEntity, x => x.MaxActiveUsagesCount);
                }
                
                await _mainDatabase.CommitChangesAsync();
                
                return ServiceResult.Success();
            },
            // default isolation level ReadCommitted is enough here because of Optimistic Concurrency
            // is implemented for Product. So ExecuteInTransaction is used just for retry.
            // Also inside ExecuteInTransaction there is possible to call CommitChanges multiple times
            // (if it is required) and be sure that whole transaction will be rolled back if any CommitChanges
            // throw exception because of Concurrency error.
            //
            // Without Optimistic Concurrency feature RepeatableRead (or Snapshot) isolation level should be passed here
            // if (updateProductModel.MaxActiveUsagesCount < updateProductModel.CurrentActiveUsagesCount) option is used
            // or Serializable (to also lock ProductUsages for insert/delete)
            // if (updateProductModel.MaxActiveUsagesCount < productEntity.ProductUsages.Count) option is used (with
            // ProductUsages included)
            delayBetweenRetries: TimeSpan.FromMilliseconds(300));

        return result ?? ServiceResult.Error("Error occured during DB transaction executing");
    }

    public async Task<IServiceResult> DeleteProductAsync(Guid id)
    {
        var productEntity = await _mainDatabase.Products.GetByIdAsync(id);
        if (productEntity == null)
        {
            return ServiceResult.Error("Product was not found");
        }
        
        _mainDatabase.Products.Delete(productEntity);
        await _mainDatabase.CommitChangesAsync();

        return ServiceResult.Success();
    }
}