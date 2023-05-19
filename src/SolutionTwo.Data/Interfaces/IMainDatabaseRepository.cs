using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Data.Interfaces;

public interface IMainDatabaseRepository
{
    IUserRepository Users { get; }

    Task SaveAsync();
    
    void Save();
}