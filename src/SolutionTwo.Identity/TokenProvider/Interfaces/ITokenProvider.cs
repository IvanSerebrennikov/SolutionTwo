namespace SolutionTwo.Identity.TokenProvider.Interfaces;

public interface ITokenProvider
{
    string GenerateAuthToken(params (string, string)[] claims);
}