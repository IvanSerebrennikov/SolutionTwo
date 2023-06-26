using Microsoft.Data.SqlClient;

namespace SolutionTwo.Api.Tests.Helpers;

public static class DatabaseHelper
{
    public static void ClearAllIntegrationDatabaseTables()
    {
        var connectionStrings = ConfigurationHelper.ConnectionStrings;
        var connectionString = connectionStrings.MainDatabaseConnection;

        var queryString = @"DECLARE @DeleteFromTables NVARCHAR(max) = ''
                            SELECT @DeleteFromTables += 'DELETE FROM ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + '; '
                            FROM INFORMATION_SCHEMA.TABLES
                            WHERE TABLE_NAME NOT IN ('__EFMigrationsHistory')
                            EXEC(@DeleteFromTables);";

        using var connection = new SqlConnection(connectionString);
        var command = new SqlCommand(queryString, connection);
        command.Connection.Open();
        command.ExecuteNonQuery();
    }
}