using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolutionTwo.Data.Common.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.Common.UnitOfWork;

public class BaseUnitOfWork : IBaseUnitOfWork
{
    protected readonly DbContext Context;
    private readonly ILogger _logger;

    protected BaseUnitOfWork(DbContext context, ILogger logger)
    {
        Context = context;
        _logger = logger;
    }
    
    public async Task CommitChangesAsync()
    {
        await Context.SaveChangesAsync();
    }

    public void CommitChanges()
    {
        Context.SaveChanges();
    }
    
    public TResult? ExecuteInTransactionWithRetry<TResult>(
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
            using var transaction = Context.Database.BeginTransaction(isolationLevel);
            
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
                
                Context.ChangeTracker.Clear();

                if (delayBetweenRetries != default)
                    Thread.Sleep(delayBetweenRetries);
                
                attempt++;
            }
        }

        return result;
    }
    
    public async Task<TResult?> ExecuteInTransactionWithRetryAsync<TResult>(
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
            await using var transaction = await Context.Database.BeginTransactionAsync(isolationLevel);
            
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
                
                Context.ChangeTracker.Clear();
                
                if (delayBetweenRetries != default)
                    await Task.Delay(delayBetweenRetries);
                
                attempt++;
            }
        }

        return result;
    }

    public async Task<int> ExecuteSqlRawAsync(string rawSql, params object[] parameters)
    {
        return await Context.Database.ExecuteSqlRawAsync(rawSql, parameters);
    }
}