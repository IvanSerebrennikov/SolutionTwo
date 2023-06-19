using SolutionTwo.Data.Common.UnitOfWork.Interfaces;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

public interface IMainDatabase : IBaseUnitOfWork
{
    ITenantRepository Tenants { get; }
    
    IUserRepository Users { get; }
    
    IRoleRepository Roles { get; }
    
    IRefreshTokenRepository RefreshTokens { get; }
    
    IProductRepository Products { get; }
    
    IProductUsageRepository ProductUsages { get; }
}