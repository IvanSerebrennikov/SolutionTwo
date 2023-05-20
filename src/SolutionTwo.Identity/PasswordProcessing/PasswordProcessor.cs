using Microsoft.AspNetCore.Identity;
using SolutionTwo.Identity.PasswordProcessing.Interfaces;

namespace SolutionTwo.Identity.PasswordProcessing;

public class PasswordProcessor : IPasswordProcessor
{
    private readonly IPasswordHasher<object> _passwordHasher;

    public PasswordProcessor(IPasswordHasher<object> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(object user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyHashedPassword(object user, string hashedPassword, string providedPassword)
    {
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

        return verificationResult != PasswordVerificationResult.Failed;
    }
}