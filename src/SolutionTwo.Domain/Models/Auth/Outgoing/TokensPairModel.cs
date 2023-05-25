namespace SolutionTwo.Domain.Models.Auth.Outgoing;

public class TokensPairModel
{
    public TokensPairModel(string authToken, string refreshToken)
    {
        AuthToken = authToken;
        RefreshToken = refreshToken;
    }

    public string AuthToken { get; }

    public string RefreshToken { get; }
}