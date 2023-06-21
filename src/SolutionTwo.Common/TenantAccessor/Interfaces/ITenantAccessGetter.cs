namespace SolutionTwo.Common.TenantAccessor.Interfaces;

public interface ITenantAccessGetter
{
    Guid? TenantId { get; }
    
    bool AllTenantsAccessible { get; }
}