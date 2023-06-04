namespace SolutionTwo.Data.Common.MultiTenancy.Entities.Interfaces;

public interface IOwnedByTenantEntity
{
    Guid TenantId { get; set; }
}