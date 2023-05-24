namespace SolutionTwo.Domain.Models.User.Input;

public class CreateUserModel
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool IsValid(out string errorMessage)
    {
        var isValid = !string.IsNullOrEmpty(FirstName) &&
                      !string.IsNullOrEmpty(LastName) &&
                      !string.IsNullOrEmpty(Username) &&
                      !string.IsNullOrEmpty(Password);

        errorMessage = isValid ? "" : "Passed data is invalid. All properties are required.";

        return isValid;
    }
}