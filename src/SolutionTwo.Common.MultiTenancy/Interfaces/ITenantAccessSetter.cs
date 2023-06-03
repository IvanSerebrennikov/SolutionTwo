namespace SolutionTwo.Common.MultiTenancy;

public interface ITenantAccessSetter
{
    void SetAccessToTenant(Guid tenantId);
    
    void SetAccessToAllTenants();
}