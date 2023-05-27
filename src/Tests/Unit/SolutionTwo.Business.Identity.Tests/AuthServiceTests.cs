using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SolutionTwo.Business.Common.PasswordManager;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenManager;
using SolutionTwo.Business.Identity.TokenManager.Interfaces;
using SolutionTwo.Business.Tests.InMemoryRepositories;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Tests;

public class AuthServiceTests
{
    private IAuthService _authService = null!;
    
    private ITokenManager _tokenManager = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IUserRepository _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        var identityConfiguration = new IdentityConfiguration
        {
            JwtKey = Guid.NewGuid().ToString(),
            JwtAudience = "test",
            JwtIssuer = "test",
            JwtExpiresMinutes = 15,
            RefreshTokenExpiresDays = 7
        };

        _refreshTokenRepository = new InMemoryRefreshTokenRepository();

        _userRepository = new InMemoryUserRepository();
        
        var mainDatabaseMock = new Mock<IMainDatabase>();
        var mainDatabase = mainDatabaseMock.Object;

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        _tokenManager = new JwtManager(identityConfiguration, memoryCache);

        var passwordHasher = new PasswordHasher<object>();
        var passwordManager = new PasswordManager(passwordHasher);

        var loggerMock = new Mock<ILogger<AuthService>>();
        var logger = loggerMock.Object;

        _authService = new AuthService(identityConfiguration, _refreshTokenRepository, _userRepository, mainDatabase,
            _tokenManager, passwordManager, logger);
    }

    [Test]
    public void RefreshTokensPairAsyncRevokesAllActiveTokensForUserWhenCalledTwiceForSameRefreshToken()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var activeRefreshToken1 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        var activeRefreshToken2 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-2),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(5)
        };
        var activeRefreshToken3 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-6),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(1)
        };
        _userRepository.Create(user);
        _refreshTokenRepository.Create(activeRefreshToken1);
        _refreshTokenRepository.Create(activeRefreshToken2);
        _refreshTokenRepository.Create(activeRefreshToken3);

        _authService.RefreshTokensPairAsync(activeRefreshToken1.Id);
        _authService.RefreshTokensPairAsync(activeRefreshToken1.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(activeRefreshToken1.IsRevoked, Is.True);
            Assert.That(activeRefreshToken2.IsRevoked, Is.True);
            Assert.That(activeRefreshToken3.IsRevoked, Is.True);
            Assert.That(_tokenManager.IsAuthTokenRevoked(activeRefreshToken1.AuthTokenId), Is.True);
            Assert.That(_tokenManager.IsAuthTokenRevoked(activeRefreshToken2.AuthTokenId), Is.True);
            Assert.That(_tokenManager.IsAuthTokenRevoked(activeRefreshToken3.AuthTokenId), Is.True);
        });
    }
    
    [Test]
    public void RefreshTokensPairAsyncDoesNotRevokeNotActiveTokensForUserWhenCalledTwiceForSameRefreshToken()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var activeRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        var notActiveRefreshToken1 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = userId,
            IsUsed = true,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-2),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(5)
        };
        var notActiveRefreshToken2 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-8),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(-1)
        };
        _userRepository.Create(user);
        _refreshTokenRepository.Create(activeRefreshToken);
        _refreshTokenRepository.Create(notActiveRefreshToken1);
        _refreshTokenRepository.Create(notActiveRefreshToken2);

        _authService.RefreshTokensPairAsync(activeRefreshToken.Id);
        _authService.RefreshTokensPairAsync(activeRefreshToken.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(activeRefreshToken.IsRevoked, Is.True);
            Assert.That(notActiveRefreshToken1.IsRevoked, Is.False);
            Assert.That(notActiveRefreshToken2.IsRevoked, Is.False);
            Assert.That(_tokenManager.IsAuthTokenRevoked(activeRefreshToken.AuthTokenId), Is.True);
            Assert.That(_tokenManager.IsAuthTokenRevoked(notActiveRefreshToken1.AuthTokenId), Is.False);
            Assert.That(_tokenManager.IsAuthTokenRevoked(notActiveRefreshToken2.AuthTokenId), Is.False);
        });
    }
    
    [Test]
    public void RefreshTokensPairAsyncDoesNotRevokeActiveTokensForOtherUsersWhenCalledTwiceForSameRefreshToken()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var activeRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        var activeRefreshTokenForOtherUser1 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        var activeRefreshTokenForOtherUser2 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        _userRepository.Create(user);
        _refreshTokenRepository.Create(activeRefreshToken);
        _refreshTokenRepository.Create(activeRefreshTokenForOtherUser1);
        _refreshTokenRepository.Create(activeRefreshTokenForOtherUser2);

        _authService.RefreshTokensPairAsync(activeRefreshToken.Id);
        _authService.RefreshTokensPairAsync(activeRefreshToken.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(activeRefreshToken.IsRevoked, Is.True);
            Assert.That(activeRefreshTokenForOtherUser1.IsRevoked, Is.False);
            Assert.That(activeRefreshTokenForOtherUser2.IsRevoked, Is.False);
            Assert.That(_tokenManager.IsAuthTokenRevoked(activeRefreshToken.AuthTokenId), Is.True);
            Assert.That(_tokenManager.IsAuthTokenRevoked(activeRefreshTokenForOtherUser1.AuthTokenId), Is.False);
            Assert.That(_tokenManager.IsAuthTokenRevoked(activeRefreshTokenForOtherUser2.AuthTokenId), Is.False);
        });
    }
}