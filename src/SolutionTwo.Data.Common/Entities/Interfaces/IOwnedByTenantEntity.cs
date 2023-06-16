namespace SolutionTwo.Data.Common.Entities.Interfaces;

public interface IOwnedByTenantEntity
{
    Guid TenantId { get; set; }
}