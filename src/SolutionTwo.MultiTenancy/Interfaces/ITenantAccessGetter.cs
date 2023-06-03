namespace SolutionTwo.MultiTenancy;

public interface ITenantAccessGetter
{
    bool IsInitialized { get; }
    
    Guid? TenantId { get; }
    
    bool AllTenantsAccessible { get; }
}