namespace SolutionTwo.Business.Common.PasswordHasher.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}