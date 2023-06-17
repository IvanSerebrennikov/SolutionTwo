using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.MainDatabase.UnitOfWork;

public class MainDatabase : IMainDatabase
{
    private readonly MainDatabaseContext _context;

    public MainDatabase(
        MainDatabaseContext context, 
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IProductRepository productRepository)
    {
        _context = context;
        Tenants = tenantRepository;
        Users = userRepository;
        Roles = roleRepository;
        RefreshTokens = refreshTokenRepository;
        Products = productRepository;
    }

    public ITenantRepository Tenants { get; }
    
    public IUserRepository Users { get; }
    
    public IRoleRepository Roles { get; }

    public IRefreshTokenRepository RefreshTokens { get; }
    
    public IProductRepository Products { get; }

    public async Task CommitChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void CommitChanges()
    {
        _context.SaveChanges();
    }
}