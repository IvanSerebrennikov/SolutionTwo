namespace SolutionTwo.Api.Models.Auth;

public class AuthResponse
{
    public AuthResponse(string authToken, string refreshToken)
    {
        AuthToken = authToken;
        RefreshToken = refreshToken;
    }
    
    public string AuthToken { get; set; }

    public string RefreshToken { get; set; }
}