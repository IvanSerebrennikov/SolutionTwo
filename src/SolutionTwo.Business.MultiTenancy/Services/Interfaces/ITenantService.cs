using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.MultiTenancy.Models.Tenant.Incoming;
using SolutionTwo.Business.MultiTenancy.Models.Tenant.Outgoing;

namespace SolutionTwo.Business.MultiTenancy.Services.Interfaces;

public interface ITenantService
{
    Task<TenantModel?> GetTenantByIdAsync(Guid id);
    
    Task<IReadOnlyList<TenantModel>> GetAllTenantsAsync();

    Task<IServiceResult<TenantModel>> CreateTenantAsync(CreateTenantModel createTenantModel);
}