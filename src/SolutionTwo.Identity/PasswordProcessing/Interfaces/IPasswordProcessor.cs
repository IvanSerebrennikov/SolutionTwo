namespace SolutionTwo.Identity.PasswordProcessing.Interfaces;

public interface IPasswordProcessor
{
    string HashPassword(Guid userId, string password);

    bool VerifyHashedPassword(Guid userId, string hashedPassword, string providedPassword);
}