using SolutionTwo.Common.Interfaces;

namespace SolutionTwo.Data.Common.Configuration;

public class ConnectionStrings : IVerifiableConfiguration
{
    public string? MainDatabaseConnection { get; set; }
}