using SolutionTwo.Common.TenantAccessor.Interfaces;

namespace SolutionTwo.Common.TenantAccessor;

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