namespace SolutionTwo.Common.MultiTenancy.Interfaces;

public interface ITenantAccessSetter
{
    void SetAccessToTenant(Guid tenantId);
    
    void SetAccessToAllTenants();
}