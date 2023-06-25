using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.FunctionApp.DataBaseOperations.Functions;

public class ExpiredRefreshTokensDeleteFunction
{
    private readonly IMainDatabase _mainDatabase;

    public ExpiredRefreshTokensDeleteFunction(IMainDatabase mainDatabase)
    {
        _mainDatabase = mainDatabase;
    }

    // every day at 03:00:00
    [FunctionName("ExpiredRefreshTokensDeleteFunction")]
    public async Task RunAsync([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation(
            $"C# ExpiredRefreshTokensDelete Timer trigger function execution started at: {DateTime.UtcNow}");

        try
        {
            var deletedRefreshTokensCount = await DeleteAllExpiredRefreshTokensAsync();
            
            log.LogInformation("Deleted refresh tokens count: {count}", deletedRefreshTokensCount);
        }
        catch (Exception e)
        {
            log.LogError(e, "Exception occured during expired refresh tokens deletion");
        }

        log.LogInformation(
            $"C# ExpiredRefreshTokensDelete Timer trigger function execution finished at: {DateTime.UtcNow}");
    }

    private async Task<int> DeleteAllExpiredRefreshTokensAsync()
    {
        var rawSql = "DELETE FROM RefreshTokens WHERE ExpiresDateTimeUtc < GETUTCDATE()";

        return await _mainDatabase.ExecuteSqlRawAsync(rawSql);
    }
}