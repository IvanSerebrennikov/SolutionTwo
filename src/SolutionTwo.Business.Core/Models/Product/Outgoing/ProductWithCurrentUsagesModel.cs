using SolutionTwo.Data.MainDatabase.Entities;

namespace SolutionTwo.Business.Core.Models.Product.Outgoing;

public class ProductWithCurrentUsagesModel
{
    public ProductWithCurrentUsagesModel(ProductEntity productEntity)
    {
        Id = productEntity.Id;
        Name = productEntity.Name;
        MaxNumberOfSimultaneousUsages = productEntity.MaxNumberOfSimultaneousUsages;
        CurrentUsages = productEntity.ProductUsages.Select(x => new ProductUsageModel(x)).ToList();
    }
    
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public int MaxNumberOfSimultaneousUsages { get; set; }

    public List<ProductUsageModel> CurrentUsages { get; set; }
}