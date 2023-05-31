using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

public interface IMainDatabase
{
    IUserRepository Users { get; }
    
    IRefreshTokenRepository RefreshTokens { get; }
    
    Task CommitChangesAsync();

    void CommitChanges();
}