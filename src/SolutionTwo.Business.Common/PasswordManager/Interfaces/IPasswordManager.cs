namespace SolutionTwo.Business.Common.PasswordManager.Interfaces;

public interface IPasswordManager
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}