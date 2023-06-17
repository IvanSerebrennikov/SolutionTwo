using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

public interface IMainDatabase
{
    ITenantRepository Tenants { get; }
    
    IUserRepository Users { get; }
    
    IRoleRepository Roles { get; }
    
    IRefreshTokenRepository RefreshTokens { get; }
    
    IProductRepository Products { get; }
    
    Task CommitChangesAsync();

    void CommitChanges();
}