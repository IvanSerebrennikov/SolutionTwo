using System.Data;
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

    TResult? ExecuteInTransaction<TResult>(
        Func<TResult> funcToBeExecuted,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int maxRetryCount = 3,
        TimeSpan delayBetweenRetries = default);

    Task<TResult?> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> funcToBeExecuted,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int maxRetryCount = 3,
        TimeSpan delayBetweenRetries = default);
}