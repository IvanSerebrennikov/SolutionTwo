namespace SolutionTwo.Data.Common.Features.MultiTenancy;

public interface IOwnedByTenantEntity
{
    Guid TenantId { get; set; }
}