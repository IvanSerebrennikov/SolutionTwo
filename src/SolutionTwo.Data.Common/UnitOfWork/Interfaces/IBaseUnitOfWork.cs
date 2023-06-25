using System.Data;

namespace SolutionTwo.Data.Common.UnitOfWork.Interfaces;

public interface IBaseUnitOfWork
{
    Task CommitChangesAsync();

    void CommitChanges();

    TResult? ExecuteInTransactionWithRetry<TResult>(
        Func<TResult> funcToBeExecuted,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int maxRetryCount = 3,
        TimeSpan delayBetweenRetries = default);

    Task<TResult?> ExecuteInTransactionWithRetryAsync<TResult>(
        Func<Task<TResult>> funcToBeExecuted,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int maxRetryCount = 3,
        TimeSpan delayBetweenRetries = default);

    Task<int> ExecuteSqlRawAsync(string rawSql, params object[] parameters);
}