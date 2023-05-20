namespace SolutionTwo.Identity.PasswordProcessing.Interfaces;

public interface IPasswordProcessor
{
    string HashPassword(object user, string password);

    bool VerifyHashedPassword(object user, string hashedPassword, string providedPassword);
}