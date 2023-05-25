namespace SolutionTwo.Domain.Models.User.Incoming;

public class CreateUserModel
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }
}