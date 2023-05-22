namespace SolutionTwo.Identity.TokenProvider.Interfaces;

public interface ITokenProvider
{
    string GenerateAuthToken(List<(string, string)> claims);
}