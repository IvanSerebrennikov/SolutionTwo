using SolutionTwo.Data.MainDatabase.Entities;

namespace SolutionTwo.Business.Core.Models.Product.Outgoing;

public class ProductUsageModel
{
    public ProductUsageModel(ProductUsageEntity productUsageEntity)
    {
        Id = productUsageEntity.Id;
        UserId = productUsageEntity.UserId;
        UsageStartDateTimeUtc = productUsageEntity.UsageStartDateTimeUtc;
    }
    
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime UsageStartDateTimeUtc { get; set; }
}