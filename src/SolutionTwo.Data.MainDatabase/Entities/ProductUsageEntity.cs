using SolutionTwo.Data.Common.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Entities;

public class ProductUsageEntity : IIdentifiablyEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid UserId { get; set; }

    public DateTime UsageStartDateTimeUtc { get; set; }

    public DateTime? ReleaseDateTimeUtc { get; set; }

    public bool? IsForceRelease { get; set; }
    
    public ProductEntity? Product { get; set; }

    public UserEntity? User { get; set; }
}