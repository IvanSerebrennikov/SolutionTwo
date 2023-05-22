namespace SolutionTwo.Identity.PasswordManaging.Interfaces;

public interface IPasswordManager
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}