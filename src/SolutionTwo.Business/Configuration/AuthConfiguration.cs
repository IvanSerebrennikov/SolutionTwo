using SolutionTwo.Common.Interfaces;

namespace SolutionTwo.Business.Configuration;

public class AuthConfiguration : IVerifiableConfiguration
{
    public int? RefreshTokenExpiresDays { get; set; }
}