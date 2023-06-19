using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Core.Models.Product.Incoming;
using SolutionTwo.Business.Core.Models.Product.Outgoing;

namespace SolutionTwo.Business.Core.Services.Interfaces;

public interface IProductService
{
    Task<IServiceResult> UseProductAsync(Guid id);
    
    Task<IServiceResult> ReleaseProductAsync(Guid id);
    
    Task<ProductWithActiveUsagesModel?> GetProductWithActiveUsagesByIdAsync(Guid id);
    
    Task<IReadOnlyList<ProductWithActiveUsagesModel>> GetAllProductsWithActiveUsagesAsync();
    
    Task<IServiceResult<ProductWithActiveUsagesModel>> CreateProductAsync(CreateProductModel createProductModel);
    
    Task<IServiceResult> UpdateProductAsync(UpdateProductModel updateProductModel);

    Task<IServiceResult> DeleteProductAsync(Guid id);
}