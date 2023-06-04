namespace SolutionTwo.Common.MultiTenancy;

public interface ITenantAccessGetter
{
    Guid? TenantId { get; }
    
    bool AllTenantsAccessible { get; }
}