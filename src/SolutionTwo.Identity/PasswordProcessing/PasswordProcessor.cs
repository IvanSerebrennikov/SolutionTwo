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

    public string HashPassword(Guid userId, string password)
    {
        return _passwordHasher.HashPassword(new { Id = userId }, password);
    }

    public bool VerifyHashedPassword(Guid userId, string hashedPassword, string providedPassword)
    {
        var verificationResult = _passwordHasher.VerifyHashedPassword(new { Id = userId }, hashedPassword, providedPassword);

        return verificationResult != PasswordVerificationResult.Failed;
    }
}