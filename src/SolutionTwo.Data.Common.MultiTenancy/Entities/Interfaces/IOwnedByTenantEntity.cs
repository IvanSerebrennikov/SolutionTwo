namespace SolutionTwo.Data.Common.MultiTenancy.Entities.Interfaces;

public interface IOwnedByTenantEntity
{
    public Guid TenantId { get; set; }
}