using Microsoft.AspNetCore.Identity;
using SolutionTwo.Business.Common.PasswordManager.Interfaces;

namespace SolutionTwo.Business.Common.PasswordManager;

public class PasswordManager : IPasswordManager
{
    private readonly IPasswordHasher<object> _passwordHasher;
    
    // IPasswordHasher need it, but do nothing with it
    private readonly object _dummyUser = new();

    public PasswordManager(IPasswordHasher<object> passwordHasher)
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