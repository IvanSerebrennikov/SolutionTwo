using SolutionTwo.Data.Interfaces;
using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Data;

public class MainDatabaseRepository : IMainDatabaseRepository
{
    public IUserRepository Users { get; }

    public MainDatabaseRepository(IUserRepository userRepository)
    {
        Users = userRepository;
    }
    
    public void Save()
    {
        throw new NotImplementedException();
    }
}