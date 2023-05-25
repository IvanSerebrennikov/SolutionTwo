namespace SolutionTwo.Identity.PasswordManagement.Interfaces;

public interface IPasswordManager
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}