using System.Data;
using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Core.Models.Product.Incoming;
using SolutionTwo.Business.Core.Models.Product.Outgoing;
using SolutionTwo.Business.Core.Services.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Core.Services;

public class ProductService : IProductService
{
    private readonly IMainDatabase _mainDatabase;
    private readonly ILogger<UserService> _logger;

    public ProductService(IMainDatabase mainDatabase, ILogger<UserService> logger)
    {
        _mainDatabase = mainDatabase;
        _logger = logger;
    }

    public async Task<ProductWithCurrentUsagesModel?> GetProductWithCurrentUsagesByIdAsync(Guid id)
    {
        var productEntity = await _mainDatabase.Products.GetByIdAsync(id,
            include: x => x.ProductUsages.Where(u => u.ReleaseDateTimeUtc == null));

        return productEntity != null ? new ProductWithCurrentUsagesModel(productEntity) : null;
    }

    public async Task<IReadOnlyList<ProductWithCurrentUsagesModel>> GetAllProductsWithCurrentUsagesAsync()
    {
        var productEntities = await _mainDatabase.Products.GetAsync(
            include: x => x.ProductUsages.Where(u => u.ReleaseDateTimeUtc == null));
        var productModels = productEntities.Select(x => new ProductWithCurrentUsagesModel(x)).ToList();

        return productModels;
    }

    public async Task<IServiceResult<ProductWithCurrentUsagesModel>> CreateProductAsync(
        CreateProductModel createProductModel)
    {
        var existingProductWithSameName =
            await _mainDatabase.Products.GetSingleAsync(x => x.Name == createProductModel.Name);
        if (existingProductWithSameName != null)
        {
            return ServiceResult<ProductWithCurrentUsagesModel>.Error(
                $"Product with name '{createProductModel.Name}' already exists");
        }

        var productEntity = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = createProductModel.Name,
            MaxNumberOfSimultaneousUsages = createProductModel.MaxNumberOfSimultaneousUsages
        };

        _mainDatabase.Products.Create(productEntity);

        await _mainDatabase.CommitChangesAsync();

        var model = new ProductWithCurrentUsagesModel(productEntity);

        return ServiceResult<ProductWithCurrentUsagesModel>.Success(model);
    }

    public async Task<IServiceResult> UpdateProductAsync(
        UpdateProductModel updateProductModel)
    {
        var result = await _mainDatabase.ExecuteInTransactionAsync(async () =>
            {
                var productEntity = await _mainDatabase.Products.GetByIdAsync(updateProductModel.Id,
                    include: x => x.ProductUsages.Where(u => u.ReleaseDateTimeUtc == null));
                if (productEntity == null)
                {
                    return ServiceResult.Error(
                        "Product was not found");
                }

                if (updateProductModel.MaxNumberOfSimultaneousUsages < productEntity.ProductUsages.Count)
                {
                    return ServiceResult.Error(
                        "Product has more current active usages " +
                        "than provided in new value for MaxNumberOfSimultaneousUsages");
                }

                productEntity.Name = updateProductModel.Name;
                productEntity.MaxNumberOfSimultaneousUsages = updateProductModel.MaxNumberOfSimultaneousUsages;

                _mainDatabase.Products.Update(productEntity, x => x.Name, x => x.MaxNumberOfSimultaneousUsages);

                await _mainDatabase.CommitChangesAsync();
                
                return ServiceResult.Success();
            },
            isolationLevel: IsolationLevel.RepeatableRead,
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