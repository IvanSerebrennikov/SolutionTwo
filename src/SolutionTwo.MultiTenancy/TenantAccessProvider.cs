namespace SolutionTwo.MultiTenancy;

public class TenantAccessProvider : ITenantAccessGetter, ITenantAccessSetter
{
    public bool IsInitialized { get; private set; }
    
    public Guid? TenantId { get; private set; }
    
    public bool AllTenantsAccessible { get; private set; }
    
    public void SetAccessToTenant(Guid tenantId)
    {
        IsInitialized = true;
        AllTenantsAccessible = false;
        TenantId = tenantId;
    }

    public void SetAccessToAllTenants()
    {
        IsInitialized = true;
        AllTenantsAccessible = true;
        TenantId = null;
    }
}