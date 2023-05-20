namespace SolutionTwo.Api.Models.Auth;

public class AuthRequest
{
    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool IsValid() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}