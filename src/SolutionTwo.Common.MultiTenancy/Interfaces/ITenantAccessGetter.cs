namespace SolutionTwo.Common.MultiTenancy;

public interface ITenantAccessGetter
{
    bool IsInitialized { get; }
    
    Guid? TenantId { get; }
    
    bool AllTenantsAccessible { get; }
}