using SolutionTwo.Common.MultiTenancy.Interfaces;

namespace SolutionTwo.Common.MultiTenancy;

public class TenantAccessor : ITenantAccessGetter, ITenantAccessSetter
{
    public Guid? TenantId { get; private set; }
    
    public bool AllTenantsAccessible { get; private set; }
    
    public void SetAccessToTenant(Guid tenantId)
    {
        AllTenantsAccessible = false;
        TenantId = tenantId;
    }

    public void SetAccessToAllTenants()
    {
        AllTenantsAccessible = true;
        TenantId = null;
    }
}