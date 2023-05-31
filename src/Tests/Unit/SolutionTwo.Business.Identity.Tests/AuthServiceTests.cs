using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.PasswordHasher;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Models.Auth.Incoming;
using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.InMemory.MainDatabase;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Tests;

public class AuthServiceTests
{
    private IAuthService _authService = null!;
    private IMainDatabase _mainDatabase = null!;
    private const int RefreshTokenExpiresDays = 7;

    private UserEntity _user1 = null!;
    private UserEntity _user2 = null!;
    private UserEntity _user3 = null!;

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
        
        var userRepository = new InMemoryUserRepository();
        var refreshTokenRepository = new InMemoryRefreshTokenRepository();
        var mainDatabaseMock = new Mock<IMainDatabase>();
        mainDatabaseMock.Setup(x => x.Users).Returns(userRepository);
        mainDatabaseMock.Setup(x => x.RefreshTokens).Returns(refreshTokenRepository);
        _mainDatabase = mainDatabaseMock.Object;
        
        var tokenProvider = new JwtProvider(identityConfiguration);

        var loggerMock = new Mock<ILogger<AuthService>>();
        var logger = loggerMock.Object;

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        var passwordHasher = new PasswordHasher(new PasswordHasher<object>());
        
        _authService = new AuthService(identityConfiguration, _mainDatabase,
            tokenProvider, memoryCache, logger, passwordHasher);

        var randomFaceRolledDataEnteredForUser1ToAllFields = "q1";
        _user1 = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = randomFaceRolledDataEnteredForUser1ToAllFields,
            LastName = randomFaceRolledDataEnteredForUser1ToAllFields,
            Username = randomFaceRolledDataEnteredForUser1ToAllFields,
            PasswordHash = passwordHasher.HashPassword(randomFaceRolledDataEnteredForUser1ToAllFields),
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var randomFaceRolledDataEnteredForUser2ToAllFields = "x2";
        _user2 = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = randomFaceRolledDataEnteredForUser2ToAllFields,
            LastName = randomFaceRolledDataEnteredForUser2ToAllFields,
            Username = randomFaceRolledDataEnteredForUser2ToAllFields,
            PasswordHash = passwordHasher.HashPassword(randomFaceRolledDataEnteredForUser2ToAllFields),
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        var randomFaceRolledDataEnteredForUser3ToAllFields = "e3";
        _user3 = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = randomFaceRolledDataEnteredForUser3ToAllFields,
            LastName = randomFaceRolledDataEnteredForUser3ToAllFields,
            Username = randomFaceRolledDataEnteredForUser3ToAllFields,
            PasswordHash = passwordHasher.HashPassword(randomFaceRolledDataEnteredForUser3ToAllFields),
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        _mainDatabase.Users.Create(_user1);
        _mainDatabase.Users.Create(_user2);
        _mainDatabase.Users.Create(_user3);
    }

    // TODO: ValidateCredentialsAndCreateTokensPairAsync tests
    
    // TODO: VerifyAuthTokenAndGetPrincipal tests
    
    [Test]
    public async Task RefreshTokensPairAsyncReturnsSuccessAndCreatesNewActiveRefreshTokenForUser()
    {
        var createTokensResult =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;
        
        var refreshTokensResult = await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var newTokensPair = refreshTokensResult.Data;
        var newActiveRefreshToken = (newTokensPair != null
            ? await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
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
            Assert.That(newActiveRefreshToken.UserId, Is.EqualTo(_user1.Id));
            Assert.That(newActiveRefreshToken.ExpiresDateTimeUtc,
                Is.GreaterThan(DateTime.UtcNow.AddDays(RefreshTokenExpiresDays).AddMinutes(-1)));
        });
    }

    [Test]
    public async Task RefreshTokensPairAsyncReturnsSuccessAndMarksProvidedActiveRefreshTokenAsUsed()
    {
        var createTokensResult =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;

        var refreshTokensResult = await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var providedRefreshToken = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
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
        var createTokensResult =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;
        
        await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
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
        var createTokensResult1 =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair1 = createTokensResult1.Data!.Tokens;
        var createTokensResult2 =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair2 = createTokensResult2.Data!.Tokens;
        var createTokensResult3 =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair3 = createTokensResult3.Data!.Tokens;

        await _authService.RefreshTokensPairAsync(tokensPair1.RefreshToken);
        await _authService.RefreshTokensPairAsync(tokensPair1.RefreshToken);
        var providedRefreshToken = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair1.RefreshToken);
        var otherActiveRefreshToken1 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair2.RefreshToken);
        var otherActiveRefreshToken2 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair3.RefreshToken);
        var verificationResult1 = _authService.VerifyAuthTokenAndGetPrincipal(tokensPair1.AuthToken);
        var verificationResult2 = _authService.VerifyAuthTokenAndGetPrincipal(tokensPair2.AuthToken);
        var verificationResult3 = _authService.VerifyAuthTokenAndGetPrincipal(tokensPair3.AuthToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(providedRefreshToken, Is.Not.Null);
            Assert.That(otherActiveRefreshToken1, Is.Not.Null);
            Assert.That(otherActiveRefreshToken2, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(providedRefreshToken!.IsRevoked, Is.True);
            Assert.That(otherActiveRefreshToken1!.IsRevoked, Is.True);
            Assert.That(otherActiveRefreshToken2!.IsRevoked, Is.True);
            Assert.That(verificationResult1.IsSucceeded, Is.False);
            Assert.That(verificationResult2.IsSucceeded, Is.False);
            Assert.That(verificationResult3.IsSucceeded, Is.False);
        });
    }

    [Test]
    public async Task RefreshTokensPairAsyncDoesNotRevokeNotActiveTokensForUserWhenCalledTwiceForSameRefreshToken()
    {
        // provided
        var createTokensResult =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;
        
        // used
        var createOtherTokensResult1 =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var otherTokensPair1 = createOtherTokensResult1.Data!.Tokens;
        var otherRefreshToken1 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair1.RefreshToken);
        otherRefreshToken1!.IsUsed = true;
        
        // expired
        var createOtherTokensResult2 =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var otherTokensPair2 = createOtherTokensResult2.Data!.Tokens;
        var otherRefreshToken2 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair2.RefreshToken);
        otherRefreshToken2!.ExpiresDateTimeUtc =
            otherRefreshToken2.ExpiresDateTimeUtc.AddDays(-RefreshTokenExpiresDays);

        await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var verificationResult1 = _authService.VerifyAuthTokenAndGetPrincipal(otherTokensPair1.AuthToken);
        var verificationResult2 = _authService.VerifyAuthTokenAndGetPrincipal(otherTokensPair2.AuthToken);

        Assert.Multiple(() =>
        {
            Assert.That(otherRefreshToken1.IsRevoked, Is.False);
            Assert.That(otherRefreshToken2.IsRevoked, Is.False);
            Assert.That(verificationResult1.IsSucceeded, Is.True);
            Assert.That(verificationResult2.IsSucceeded, Is.True);
        });
    }
    
    [Test]
    public async Task RefreshTokensPairAsyncDoesNotRevokeActiveTokensForOtherUsersWhenCalledTwiceForSameRefreshToken()
    {
        var createTokensResult =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;
        var createOtherTokensResult1 =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user2.GetCredentials());
        var otherTokensPair1 = createOtherTokensResult1.Data!.Tokens;
        var createOtherTokensResult2 =
            await _authService.ValidateCredentialsAndCreateTokensPairAsync(_user3.GetCredentials());
        var otherTokensPair2 = createOtherTokensResult2.Data!.Tokens;

        await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        await _authService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var otherActiveRefreshToken1 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair1.RefreshToken);
        var otherActiveRefreshToken2 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair2.RefreshToken);
        var verificationResult1 = _authService.VerifyAuthTokenAndGetPrincipal(otherTokensPair1.AuthToken);
        var verificationResult2= _authService.VerifyAuthTokenAndGetPrincipal(otherTokensPair2.AuthToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(otherActiveRefreshToken1, Is.Not.Null);
            Assert.That(otherActiveRefreshToken2, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(otherActiveRefreshToken1!.IsRevoked, Is.False);
            Assert.That(otherActiveRefreshToken2!.IsRevoked, Is.False);
            Assert.That(verificationResult1.IsSucceeded, Is.True);
            Assert.That(verificationResult2.IsSucceeded, Is.True);
        });
    }
}

internal static class AuthServiceTestsHelpers
{
    public static UserCredentialsModel GetCredentials(this UserEntity userEntity)
    {
        return new UserCredentialsModel
        {
            Username = userEntity.Username,
            Password = userEntity.Username
        };
    }
}