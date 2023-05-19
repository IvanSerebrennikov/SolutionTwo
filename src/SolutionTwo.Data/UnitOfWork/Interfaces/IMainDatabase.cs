using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Data.UnitOfWork.Interfaces;

public interface IMainDatabase
{
    IUserRepository Users { get; }

    Task SaveAsync();
    
    void Save();
}