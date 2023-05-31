namespace SolutionTwo.Business.Identity.Models.Auth.Outgoing;

public class AuthResult
{
    public AuthResult(TokensPairModel tokens, string firstName, string lastName)
    {
        Tokens = tokens;
        FirstName = firstName;
        LastName = lastName;
    }

    public TokensPairModel Tokens { get; }

    public string FirstName { get; }

    public string LastName { get; }
}