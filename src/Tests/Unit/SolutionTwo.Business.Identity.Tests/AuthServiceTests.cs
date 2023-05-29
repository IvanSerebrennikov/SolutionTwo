using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;
using SolutionTwo.Business.Tests.InMemoryRepositories;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Tests;

public class AuthServiceTests
{
    private IAuthService _authService = null!;
    
    private ITokenProvider _tokenProvider = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IUserRepository _userRepository = null!;
    private const int RefreshTokenExpiresDays = 7;

    [SetUp]
    public void Setup()
    {
        var identityConfiguration = new IdentityConfiguration
        {
            JwtKey = Guid.NewGuid().ToString(),
            JwtAudience = "test",
            JwtIssuer = "test",
            JwtExpiresMinutes = 15,
            RefreshTokenExpiresDays = RefreshTokenExpiresDays
        };

        _refreshTokenRepository = new InMemoryRefreshTokenRepository();

        _userRepository = new InMemoryUserRepository();
        
        var mainDatabaseMock = new Mock<IMainDatabase>();
        var mainDatabase = mainDatabaseMock.Object;
        
        _tokenProvider = new JwtProvider(identityConfiguration);

        var loggerMock = new Mock<ILogger<AuthService>>();
        var logger = loggerMock.Object;

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _authService = new AuthService(identityConfiguration, _refreshTokenRepository, _userRepository, mainDatabase,
            _tokenProvider, memoryCache, logger);
    }

    [Test]
    public async Task RefreshTokensPairAsyncReturnsSuccessAndCreatesNewActiveRefreshTokenForUser()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var providedActiveRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        _userRepository.Create(user);
        _refreshTokenRepository.Create(providedActiveRefreshToken);

        var result = await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        var newTokensPair = result.Data;
        var newActiveRefreshToken = (newTokensPair != null
            ? await _refreshTokenRepository.GetSingleAsync(x =>
                x.Id.ToString() == newTokensPair.RefreshToken)
            : await Task.FromResult<RefreshTokenEntity?>(null));
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSucceeded, Is.True);
            Assert.That(newActiveRefreshToken, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(newActiveRefreshToken?.IsRevoked, Is.False);
            Assert.That(newActiveRefreshToken?.IsUsed, Is.False);
            Assert.That(newActiveRefreshToken?.ExpiresDateTimeUtc, Is.Not.Null);
            Assert.That(newActiveRefreshToken?.UserId, Is.EqualTo(user.Id));
        });
        Assert.That(newActiveRefreshToken?.ExpiresDateTimeUtc,
            Is.GreaterThan(DateTime.UtcNow.AddDays(RefreshTokenExpiresDays).AddMinutes(-1)));
    }

    [Test]
    public async Task RefreshTokensPairAsyncReturnsSuccessAndMarksProvidedActiveRefreshTokenAsUsed()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var providedActiveRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        _userRepository.Create(user);
        _refreshTokenRepository.Create(providedActiveRefreshToken);

        var result = await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSucceeded, Is.True);
            Assert.That(providedActiveRefreshToken.IsUsed, Is.True);
        });
    }
    
    [Test]
    public async Task RefreshTokensPairAsyncReturnsErrorAndRevokesProvidedActiveTokenForUserWhenCalledTwiceForSameRefreshToken()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var providedActiveRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        _userRepository.Create(user);
        _refreshTokenRepository.Create(providedActiveRefreshToken);

        var firstCallResult = await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        var secondCallResult = await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(firstCallResult.IsSucceeded, Is.True);
            Assert.That(secondCallResult.IsSucceeded, Is.False);
            Assert.That(secondCallResult.Data, Is.Null);
            Assert.That(providedActiveRefreshToken.IsRevoked, Is.True);
            Assert.That(_authService.IsAuthTokenRevoked(providedActiveRefreshToken.AuthTokenId), Is.True);
        });
    }
    
    [Test]
    public async Task RefreshTokensPairAsyncRevokesAllActiveTokensForUserWhenCalledTwiceForSameRefreshToken()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var providedActiveRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        var activeRefreshToken2 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId =  user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-2),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(5)
        };
        var activeRefreshToken3 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId =  user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-6),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(1)
        };
        _userRepository.Create(user);
        _refreshTokenRepository.Create(providedActiveRefreshToken);
        _refreshTokenRepository.Create(activeRefreshToken2);
        _refreshTokenRepository.Create(activeRefreshToken3);

        var firstCallResult = await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        var newTokensPair = firstCallResult.Data;
        var newActiveRefreshToken =
            await _refreshTokenRepository.GetSingleAsync(x => x.Id.ToString() == newTokensPair!.RefreshToken);
        await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(newActiveRefreshToken!.IsRevoked, Is.True);
            Assert.That(activeRefreshToken2.IsRevoked, Is.True);
            Assert.That(activeRefreshToken3.IsRevoked, Is.True);
            Assert.That(_authService.IsAuthTokenRevoked(newActiveRefreshToken!.AuthTokenId), Is.True);
            Assert.That(_authService.IsAuthTokenRevoked(activeRefreshToken2.AuthTokenId), Is.True);
            Assert.That(_authService.IsAuthTokenRevoked(activeRefreshToken3.AuthTokenId), Is.True);
        });
    }
    
    [Test]
    public async Task RefreshTokensPairAsyncDoesNotRevokeNotActiveTokensForUserWhenCalledTwiceForSameRefreshToken()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var providedActiveRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        var alreadyUsedRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = user.Id,
            IsUsed = true,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-2),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(5)
        };
        var expiredRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-8),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(-1)
        };
        _userRepository.Create(user);
        _refreshTokenRepository.Create(providedActiveRefreshToken);
        _refreshTokenRepository.Create(alreadyUsedRefreshToken);
        _refreshTokenRepository.Create(expiredRefreshToken);

        await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(alreadyUsedRefreshToken.IsRevoked, Is.False);
            Assert.That(expiredRefreshToken.IsRevoked, Is.False);
            Assert.That(_authService.IsAuthTokenRevoked(alreadyUsedRefreshToken.AuthTokenId), Is.False);
            Assert.That(_authService.IsAuthTokenRevoked(expiredRefreshToken.AuthTokenId), Is.False);
        });
    }
    
    [Test]
    public async Task RefreshTokensPairAsyncDoesNotRevokeActiveTokensForOtherUsersWhenCalledTwiceForSameRefreshToken()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var otherUser1 = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var otherUser2 = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var providedActiveRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = user.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        var activeRefreshTokenForOtherUser1 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = otherUser1.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        var activeRefreshTokenForOtherUser2 = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = Guid.NewGuid(),
            UserId = otherUser2.Id,
            CreatedDateTimeUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(6)
        };
        _userRepository.Create(user);
        _userRepository.Create(otherUser1);
        _userRepository.Create(otherUser2);
        _refreshTokenRepository.Create(providedActiveRefreshToken);
        _refreshTokenRepository.Create(activeRefreshTokenForOtherUser1);
        _refreshTokenRepository.Create(activeRefreshTokenForOtherUser2);

        await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        await _authService.RefreshTokensPairAsync(providedActiveRefreshToken.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(activeRefreshTokenForOtherUser1.IsRevoked, Is.False);
            Assert.That(activeRefreshTokenForOtherUser2.IsRevoked, Is.False);
            Assert.That(_authService.IsAuthTokenRevoked(activeRefreshTokenForOtherUser1.AuthTokenId), Is.False);
            Assert.That(_authService.IsAuthTokenRevoked(activeRefreshTokenForOtherUser2.AuthTokenId), Is.False);
        });
    }
}