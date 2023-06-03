namespace SolutionTwo.MultiTenancy;

public interface ITenantAccessSetter
{
    void SetAccessToTenant(Guid tenantId);
    
    void SetAccessToAllTenants();
}