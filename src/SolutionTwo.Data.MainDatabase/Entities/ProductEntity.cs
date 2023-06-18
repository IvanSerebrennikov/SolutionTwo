using System.ComponentModel.DataAnnotations;
using SolutionTwo.Data.Common.Features.Audit;
using SolutionTwo.Data.Common.Features.MultiTenancy;
using SolutionTwo.Data.Common.Features.OptimisticConcurrency;
using SolutionTwo.Data.Common.Features.SoftDeletion;
using SolutionTwo.Data.Common.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Entities;

public class ProductEntity : 
    IIdentifiablyEntity<Guid>, 
    IOwnedByTenantEntity, 
    ISoftDeletableEntity, 
    IAuditableOnCreateEntity, 
    IAuditableOnUpdateEntity,
    IVersionedEntity
{
    public Guid Id { get; set; }
    
    public Guid TenantId { get; set; }
    
    [MaxLength(256)]
    public string Name { get; set; } = null!;

    [ConcurrencyVersionChangedOnUpdate]
    public int MaxNumberOfSimultaneousUsages { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public DateTime? LastModifiedDateTimeUtc { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
    
    [ConcurrencyVersionChangedOnUpdate]
    [IgnoreAudit]
    public DateTime? DeletedDateTimeUtc { get; set; }
    
    [ConcurrencyVersionChangedOnUpdate]
    [IgnoreAudit]
    public Guid? DeletedBy { get; set; }
    
    [ConcurrencyCheck]
    public Guid ConcurrencyVersion { get; set; }
    
    public List<ProductUsageEntity> ProductUsages { get; set; } = new();
}