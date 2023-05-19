using SolutionTwo.Data.Context;
using SolutionTwo.Data.Interfaces;
using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Data;

public class MainDatabaseRepository : IMainDatabaseRepository
{
    private readonly MainDatabaseContext _context;
    
    public IUserRepository Users { get; }

    public MainDatabaseRepository(MainDatabaseContext context, IUserRepository userRepository)
    {
        _context = context;
        Users = userRepository;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}