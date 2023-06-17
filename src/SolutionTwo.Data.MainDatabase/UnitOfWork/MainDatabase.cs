using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.MainDatabase.UnitOfWork;

public class MainDatabase : IMainDatabase
{
    private readonly MainDatabaseContext _context;
    private readonly ILogger<MainDatabase> _logger;

    public MainDatabase(
        MainDatabaseContext context, 
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IProductRepository productRepository, 
        ILogger<MainDatabase> logger)
    {
        _context = context;
        Tenants = tenantRepository;
        Users = userRepository;
        Roles = roleRepository;
        RefreshTokens = refreshTokenRepository;
        Products = productRepository;
        _logger = logger;
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
    
    public TResult? ExecuteInTransaction<TResult>(
        Func<TResult> funcToBeExecuted,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, 
        int maxRetryCount = 3, 
        TimeSpan delayBetweenRetries = default)
    {
        var attempt = 1;
        var success = false;

        TResult? result = default;
        
        while (!success && attempt <= maxRetryCount)
        {
            using var transaction = _context.Database.BeginTransaction(isolationLevel);
            
            try
            {
                result = funcToBeExecuted();

                transaction.Commit();
                
                success = true;
            }
            catch (Exception e)
                when (e is DbUpdateConcurrencyException ||
                      e.InnerException is DbUpdateException)
            {
                _logger.LogWarning(e, "Exception has been caught during transaction executing. " +
                                      "Attempt {attempt} out of {maxRetryCount}.",
                    attempt, maxRetryCount);
                
                transaction.Rollback();
                
                if (attempt == maxRetryCount)
                    throw;
                
                _context.ChangeTracker.Clear();

                if (delayBetweenRetries != default)
                    Thread.Sleep(delayBetweenRetries);
                
                attempt++;
            }
        }

        return result;
    }
    
    public async Task<TResult?> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> funcToBeExecuted,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, 
        int maxRetryCount = 3, 
        TimeSpan delayBetweenRetries = default)
    {
        var attempt = 1;
        var success = false;

        TResult? result = default;
        
        while (!success && attempt <= maxRetryCount)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
            
            try
            {
                result = await funcToBeExecuted();
                
                await transaction.CommitAsync();
                
                success = true;
            }
            catch (Exception e)
                when (e is DbUpdateConcurrencyException ||
                      e.InnerException is DbUpdateException)
            {
                _logger.LogWarning(e, "Exception has been caught during transaction executing. " +
                                      "Attempt {attempt} out of {maxRetryCount}.",
                    attempt, maxRetryCount);
                
                await transaction.RollbackAsync();
                
                if (attempt == maxRetryCount)
                    throw;
                
                _context.ChangeTracker.Clear();
                
                if (delayBetweenRetries != default)
                    await Task.Delay(delayBetweenRetries);
                
                attempt++;
            }
        }

        return result;
    }
}