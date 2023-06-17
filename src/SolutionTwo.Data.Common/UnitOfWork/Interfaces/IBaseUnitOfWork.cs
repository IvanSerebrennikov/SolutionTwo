using System.Data;

namespace SolutionTwo.Data.Common.UnitOfWork.Interfaces;

public interface IBaseUnitOfWork
{
    Task CommitChangesAsync();

    void CommitChanges();

    TResult? ExecuteInTransaction<TResult>(
        Func<TResult> funcToBeExecuted,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int maxRetryCount = 3,
        TimeSpan delayBetweenRetries = default);

    Task<TResult?> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> funcToBeExecuted,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int maxRetryCount = 3,
        TimeSpan delayBetweenRetries = default);
}