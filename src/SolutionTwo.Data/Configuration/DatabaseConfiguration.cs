using SolutionTwo.Common.Interfaces;

namespace SolutionTwo.Data.Configuration;

public class DatabaseConfiguration : IVerifiableConfiguration
{
    public string? MainDatabaseConnectionString { get; set; }
}