using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.MainDatabase.UnitOfWork;

public class MainDatabase : IMainDatabase
{
    private readonly MainDatabaseContext _context;

    public MainDatabase(
        MainDatabaseContext context, 
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _context = context;
        Users = userRepository;
        Roles = roleRepository;
        RefreshTokens = refreshTokenRepository;
    }

    public IUserRepository Users { get; }
    
    public IRoleRepository Roles { get; }

    public IRefreshTokenRepository RefreshTokens { get; }

    public async Task CommitChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void CommitChanges()
    {
        _context.SaveChanges();
    }
}