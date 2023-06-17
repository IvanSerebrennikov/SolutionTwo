using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Core.Models.Product.Incoming;
using SolutionTwo.Business.Core.Models.Product.Outgoing;

namespace SolutionTwo.Business.Core.Services.Interfaces;

public interface IProductService
{
    Task<ProductWithCurrentUsagesModel?> GetProductWithCurrentUsagesByIdAsync(Guid id);
    
    Task<IReadOnlyList<ProductWithCurrentUsagesModel>> GetAllProductsWithCurrentUsagesAsync();
    
    Task<IServiceResult<ProductWithCurrentUsagesModel>> CreateProductAsync(CreateProductModel createProductModel);
    
    Task<IServiceResult> UpdateProductAsync(UpdateProductModel updateProductModel);

    Task<IServiceResult> DeleteProductAsync(Guid id);
}