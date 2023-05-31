using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.InMemory.Repositories;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Tests;

public class AuthServiceTests
{
    private IAuthService _authService = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private const int RefreshTokenExpiresDays = 7;

    private readonly Guid _user1Id = Guid.NewGuid();
    private readonly Guid _user2Id = Guid.NewGuid();
    private readonly Guid _user3Id = Guid.NewGuid();

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

        var userRepository = new InMemoryUserRepository();
        
        var mainDatabaseMock = new Mock<IMainDatabase>();
        var mainDatabase = mainDatabaseMock.Object;
        
        var tokenProvider = new JwtProvider(identityConfiguration);

        var loggerMock = new Mock<ILogger<AuthService>>();
        var logger = loggerMock.Object;

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _authService = new AuthService(identityConfiguration, _refreshTokenRepository, userRepository, mainDatabase,
            tokenProvider, memoryCache, logger);
        
        var user1 = new UserEntity
        {
            Id = _user1Id,
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var user2 = new UserEntity
        {
            Id = _user2Id,
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var user3 = new UserEntity
        {
            Id = _user3Id,
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            PasswordHash = "PasswordHash",
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        userRepository.Create(user1);
        userRepository.Create(user2);
        userRepository.Create(user3);
    }

    // TODO: CreateTokensPairAsync tests
    
    // TODO: ValidateAuthTokenAndGetPrincipal tests
    
    [Test]
    public async Task RefreshTokensPairAsyncReturnsSuccessAndCreatesNewActiveRefreshTokenForUser()
    {
        var createTokensResult = await _authService.CreateTokensPairAsync(_user1Id);
        var tokensPair = createTokensResult.Data;
        
        var refreshTokensResult = await _authService.RefreshTokensPairAsync(tokensPair!.RefreshToken);
        var newTokensPair = refreshTokensResult.Data;
        var newActiveRefreshToken = (newTokensPair != null
            ? await _refreshTokenRepository.GetSingleAsync(x =>
                x.Id.ToString() == newTokensPair.RefreshToken)
            : await Task.FromResult<RefreshTokenEntity?>(null));
        
        Assert.Multiple(() =>
        {
            Assert.That(refreshTokensResult.IsSucceeded, Is.True);
            Assert.That(newActiveRefreshToken, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(newActiveRefreshToken!.IsRevoked, Is.False);
            Assert.That(newActiveRefreshToken.IsUsed, Is.False);
            Assert.That(newActiveRefreshToken.UserId, Is.EqualTo(_user1Id));
            Assert.That(newActiveRefreshToken.ExpiresDateTimeUtc,
                Is.GreaterThan(DateTime.UtcNow.AddDays(RefreshTokenExpiresDays).AddMinutes(-1)));
        });
    }

    [Test]
    public async Task RefreshTokensPairAsyncReturnsSuccessAndMarksProvidedActiveRefreshTokenAsUsed()
    {
        var createTokensResult = await _authService.CreateTokensPairAsync(_user1Id);
        var tokensPair = createTokensResult.Data;

        var refreshTokensResult = await _authService.RefreshTokensPairAsync(tokensPair!.RefreshToken);
        var providedRefreshToken = await _refreshTokenRepository.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair.RefreshToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(refreshTokensResult.IsSucceeded, Is.True);
            Assert.That(providedRefreshToken, Is.Not.Null);
        });
        Assert.That(providedRefreshToken!.IsUsed, Is.True);
    }
    
    [Test]
    public async Task RefreshTokensPairAsyncReturnsErrorWhenCalledTwiceForSameRefreshToken()
    {
        var createTokensResult = await _authService.CreateTokensPairAsync(_user1Id);
        var tokensPair = createTokensResult.Data;
        
        await _authService.RefreshTokensPairAsync(tokensPair!.RefreshToken);
        var secondRefreshTokensCallResult = await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);

        Assert.Multiple(() =>
        {
            Assert.That(secondRefreshTokensCallResult.IsSucceeded, Is.False);
            Assert.That(secondRefreshTokensCallResult.Data, Is.Null);
        });
    }

    [Test]
    public async Task RefreshTokensPairAsyncRevokesAllActiveTokensForUserWhenCalledTwiceForSameRefreshToken()
    {
        var createTokensResult1 = await _authService.CreateTokensPairAsync(_user1Id);
        var tokensPair1 = createTokensResult1.Data;
        var createTokensResult2 = await _authService.CreateTokensPairAsync(_user1Id);
        var tokensPair2 = createTokensResult2.Data;
        var createTokensResult3 = await _authService.CreateTokensPairAsync(_user1Id);
        var tokensPair3 = createTokensResult3.Data;

        await _authService.RefreshTokensPairAsync(tokensPair1!.RefreshToken);
        await _authService.RefreshTokensPairAsync(tokensPair1.RefreshToken);
        var providedRefreshToken = await _refreshTokenRepository.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair1.RefreshToken);
        var otherActiveRefreshToken1 = await _refreshTokenRepository.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair2!.RefreshToken);
        var otherActiveRefreshToken2 = await _refreshTokenRepository.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair3!.RefreshToken);
        var validateResult1 = _authService.ValidateAuthTokenAndGetPrincipal(tokensPair1.AuthToken);
        var validateResult2 = _authService.ValidateAuthTokenAndGetPrincipal(tokensPair2!.AuthToken);
        var validateResult3 = _authService.ValidateAuthTokenAndGetPrincipal(tokensPair3!.AuthToken);
        
        Assert.That(providedRefreshToken, Is.Not.Null);
        Assert.That(otherActiveRefreshToken1, Is.Not.Null);
        Assert.That(otherActiveRefreshToken2, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(providedRefreshToken!.IsRevoked, Is.True);
            Assert.That(otherActiveRefreshToken1!.IsRevoked, Is.True);
            Assert.That(otherActiveRefreshToken2!.IsRevoked, Is.True);
            Assert.That(validateResult1.IsSucceeded, Is.False);
            Assert.That(validateResult2.IsSucceeded, Is.False);
            Assert.That(validateResult3.IsSucceeded, Is.False);
        });
    }
    
    [Test]
    public async Task RefreshTokensPairAsyncDoesNotRevokeNotActiveTokensForUserWhenCalledTwiceForSameRefreshToken()
    {
        // provided
        var createTokensResult = await _authService.CreateTokensPairAsync(_user1Id);
        var tokensPair = createTokensResult.Data;
        
        // used
        var createOtherTokensResult1 = await _authService.CreateTokensPairAsync(_user1Id);
        var otherTokensPair1 = createOtherTokensResult1.Data;
        var otherRefreshToken1 = await _refreshTokenRepository.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair1!.RefreshToken);
        otherRefreshToken1!.IsUsed = true;
        
        // expired
        var createOtherTokensResult2 = await _authService.CreateTokensPairAsync(_user1Id);
        var otherTokensPair2 = createOtherTokensResult2.Data;
        var otherRefreshToken2 = await _refreshTokenRepository.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair2!.RefreshToken);
        otherRefreshToken2!.ExpiresDateTimeUtc =
            otherRefreshToken2.ExpiresDateTimeUtc.AddDays(-RefreshTokenExpiresDays);

        await _authService.RefreshTokensPairAsync(tokensPair!.RefreshToken);
        await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var validateResult1 = _authService.ValidateAuthTokenAndGetPrincipal(otherTokensPair1!.AuthToken);
        var validateResult2 = _authService.ValidateAuthTokenAndGetPrincipal(otherTokensPair2!.AuthToken);

        Assert.Multiple(() =>
        {
            Assert.That(otherRefreshToken1.IsRevoked, Is.False);
            Assert.That(otherRefreshToken2.IsRevoked, Is.False);
            Assert.That(validateResult1.IsSucceeded, Is.True);
            Assert.That(validateResult2.IsSucceeded, Is.True);
        });
    }
    
    [Test]
    public async Task RefreshTokensPairAsyncDoesNotRevokeActiveTokensForOtherUsersWhenCalledTwiceForSameRefreshToken()
    {
        var createTokensResult = await _authService.CreateTokensPairAsync(_user1Id);
        var tokensPair = createTokensResult.Data;
        var createOtherTokensResult1 = await _authService.CreateTokensPairAsync(_user2Id);
        var otherTokensPair1 = createOtherTokensResult1.Data;
        var createOtherTokensResult2 = await _authService.CreateTokensPairAsync(_user3Id);
        var otherTokensPair2 = createOtherTokensResult2.Data;

        await _authService.RefreshTokensPairAsync(tokensPair!.RefreshToken);
        await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var otherActiveRefreshToken1 = await _refreshTokenRepository.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair1!.RefreshToken);
        var otherActiveRefreshToken2 = await _refreshTokenRepository.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair2!.RefreshToken);
        var validateResult1 = _authService.ValidateAuthTokenAndGetPrincipal(otherTokensPair1!.AuthToken);
        var validateResult2= _authService.ValidateAuthTokenAndGetPrincipal(otherTokensPair2!.AuthToken);
        
        Assert.That(otherActiveRefreshToken1, Is.Not.Null);
        Assert.That(otherActiveRefreshToken2, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(otherActiveRefreshToken1!.IsRevoked, Is.False);
            Assert.That(otherActiveRefreshToken2!.IsRevoked, Is.False);
            Assert.That(validateResult1.IsSucceeded, Is.True);
            Assert.That(validateResult2.IsSucceeded, Is.True);
        });
    }
}