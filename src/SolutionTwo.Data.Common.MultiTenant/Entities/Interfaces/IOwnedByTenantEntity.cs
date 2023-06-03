namespace SolutionTwo.Data.Common.MultiTenant.Entities.Interfaces;

public interface IOwnedByTenantEntity
{
    public Guid TenantId { get; set; }
}