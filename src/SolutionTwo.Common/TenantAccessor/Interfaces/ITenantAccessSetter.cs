namespace SolutionTwo.Common.TenantAccessor.Interfaces;

public interface ITenantAccessSetter
{
    void SetAccessToTenant(Guid tenantId);
    
    void SetAccessToAllTenants();
}