using SolutionTwo.Data.MainDatabase.Entities;

namespace SolutionTwo.Business.Core.Models.Product.Outgoing;

public class ProductWithActiveUsagesModel
{
    public ProductWithActiveUsagesModel(ProductEntity productEntity)
    {
        Id = productEntity.Id;
        Name = productEntity.Name;
        MaxActiveUsagesCount = productEntity.MaxActiveUsagesCount;
        CurrentActiveUsagesCount = productEntity.CurrentActiveUsagesCount;
        CurrentUsages = productEntity.ProductUsages.Select(x => new ProductUsageModel(x)).ToList();
    }
    
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public int MaxActiveUsagesCount { get; set; }
    
    public int CurrentActiveUsagesCount { get; set; }

    public List<ProductUsageModel> CurrentUsages { get; set; }
}