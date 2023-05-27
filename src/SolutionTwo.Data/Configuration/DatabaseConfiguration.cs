using SolutionTwo.Common.Interfaces;

namespace SolutionTwo.Data.Configuration;

public class DatabaseConfiguration : IVerifiableConfiguration
{
    public int? CommandTimeOutInSeconds { get; set; }
}