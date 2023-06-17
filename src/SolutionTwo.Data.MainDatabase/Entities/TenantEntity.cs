using SolutionTwo.Data.Common.Features.SoftDeletion;
using SolutionTwo.Data.Common.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Entities;

public class TenantEntity : IIdentifiablyEntity<Guid>, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }
    
    public DateTime? DeletedDateTimeUtc { get; set; }
    
    public Guid? DeletedBy { get; set; }
}