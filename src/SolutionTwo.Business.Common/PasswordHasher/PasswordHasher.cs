using Microsoft.AspNetCore.Identity;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;

namespace SolutionTwo.Business.Common.PasswordHasher;

public class PasswordHasher : IPasswordHasher
{
    private readonly IPasswordHasher<object> _passwordHasher;
    
    // IPasswordHasher need it, but do nothing with it
    private readonly object _dummyUser = new();

    public PasswordHasher(IPasswordHasher<object> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(_dummyUser, password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        var verificationResult = _passwordHasher.VerifyHashedPassword(_dummyUser, hashedPassword, providedPassword);

        return verificationResult != PasswordVerificationResult.Failed;
    }
}