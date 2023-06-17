using Microsoft.Extensions.Logging;
using SolutionTwo.Data.Common.UnitOfWork;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.MainDatabase.UnitOfWork;

public class MainDatabase : BaseUnitOfWork, IMainDatabase
{
    public MainDatabase(
        MainDatabaseContext context,
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IProductRepository productRepository,
        ILogger<MainDatabase> logger) : base(context, logger)
    {
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
}