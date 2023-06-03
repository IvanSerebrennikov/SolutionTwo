using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
using SolutionTwo.Business.MultiTenancy.Models.Tenant.Incoming;
using SolutionTwo.Business.MultiTenancy.Models.Tenant.Outgoing;
using SolutionTwo.Business.MultiTenancy.Services.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.MultiTenancy.Services;

public class TenantService : ITenantService
{
    private readonly IMainDatabase _mainDatabase;
    private readonly IPasswordHasher _passwordHasher;

    public TenantService(IMainDatabase mainDatabase, IPasswordHasher passwordHasher)
    {
        _mainDatabase = mainDatabase;
        _passwordHasher = passwordHasher;
    }

    public async Task<TenantModel?> GetTenantByIdAsync(Guid id)
    {
        var tenantEntity = await _mainDatabase.Tenants.GetByIdAsync(id);

        return tenantEntity != null ? new TenantModel(tenantEntity) : null;
    }

    public async Task<IReadOnlyList<TenantModel>> GetAllTenantsAsync()
    {
        var tenantEntities = await _mainDatabase.Tenants.GetAsync();
        var tenantModels = tenantEntities.Select(x => new TenantModel(x)).ToList();

        return tenantModels;
    }

    public async Task<IServiceResult<TenantModel>> CreateTenantAsync(CreateTenantModel createTenantModel)
    {
        var role = await _mainDatabase.Roles.GetSingleAsync(x => x.Name == UserRoles.TenantAdmin);
        if (role == null)
        {
            return ServiceResult<TenantModel>.Error($"Role '{UserRoles.TenantAdmin}' was not found");
        }
        
        var tenantEntity = new TenantEntity
        {
            Id = Guid.NewGuid(),
            Name = createTenantModel.TenantName,
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        
        var hashedPassword = _passwordHasher.HashPassword(createTenantModel.AdminPassword);
        
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantEntity.Id,
            FirstName = createTenantModel.AdminFirstName,
            LastName = createTenantModel.AdminLastName,
            Username = createTenantModel.AdminUsername,
            PasswordHash = hashedPassword,
            CreatedDateTimeUtc = DateTime.UtcNow
        };

        _mainDatabase.Tenants.Create(tenantEntity);
        
        _mainDatabase.Users.Create(userEntity);
        
        _mainDatabase.Roles.AddRoleToUser(role, userEntity);
        
        await _mainDatabase.CommitChangesAsync();

        var model = new TenantModel(tenantEntity);

        return ServiceResult<TenantModel>.Success(model);
    }
}