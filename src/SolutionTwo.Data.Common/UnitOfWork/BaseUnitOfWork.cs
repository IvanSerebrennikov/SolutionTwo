using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolutionTwo.Data.Common.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.Common.UnitOfWork;

public class BaseUnitOfWork : IBaseUnitOfWork
{
    private readonly DbContext _context;
    private readonly ILogger _logger;

    protected BaseUnitOfWork(DbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }
    
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