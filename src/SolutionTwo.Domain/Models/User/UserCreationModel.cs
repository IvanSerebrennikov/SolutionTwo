namespace SolutionTwo.Domain.Models.User;

public class UserCreationModel
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool IsValid(out string errorMessage)
    {
        errorMessage = "Passed data is invalid. All properties are required.";

        return FirstName != null && LastName != null && Username != null && Password != null;
    }
}