using SolutionTwo.Common.Interfaces;

namespace SolutionTwo.Data.Configuration;

public class ConnectionStrings : IVerifiableConfiguration
{
    public string? MainDatabaseConnection { get; set; }
}