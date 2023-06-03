using SolutionTwo.Data.MainDatabase.Entities;

namespace SolutionTwo.Business.MultiTenancy.Models.Tenant.Outgoing;

public class TenantModel
{
    public TenantModel(TenantEntity tenantEntity)
    {
        Id = tenantEntity.Id;
        Name = tenantEntity.Name;
        CreatedDateTimeUtc = tenantEntity.CreatedDateTimeUtc;
    }

    public Guid Id { get; private set; }
    
    public string Name { get; private set; }

    public DateTime CreatedDateTimeUtc { get; private set; }
}