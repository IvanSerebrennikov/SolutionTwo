using SolutionTwo.Common.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Configuration;

public class MainDatabaseConfiguration : IVerifiableConfiguration
{
    public int? CommandTimeOutInSeconds { get; set; }
}