namespace SolutionTwo.Common.MultiTenancy.Interfaces;

public interface ITenantAccessGetter
{
    Guid? TenantId { get; }
    
    bool AllTenantsAccessible { get; }
}