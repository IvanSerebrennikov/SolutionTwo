using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Models.Auth.Incoming;
using SolutionTwo.Business.Identity.Services;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider;
using SolutionTwo.Business.Identity.TokenStore.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.InMemory.MainDatabase;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Tests;

public class IdentityServiceTests
{
    private IIdentityService _identityService = null!;
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
        
        // used real JwtProvider to simplify logic,
        // ideally ITokenProvider Mock should be used and configured in each test method
        var tokenProvider = new JwtProvider(identityConfiguration);

        var loggerMock = new Mock<ILogger<IdentityService>>();
        var logger = loggerMock.Object;

        var revokedTokens = new List<Guid>();
        var revokedTokenStoreMock = new Mock<IRevokedTokenStore>();
        revokedTokenStoreMock.Setup(x => x.RevokeAuthToken(It.IsAny<Guid>())).Callback((Guid authTokenId) =>
        {
            revokedTokens.Add(authTokenId);
        });
        revokedTokenStoreMock.Setup(x => x.IsAuthTokenRevoked(It.IsAny<Guid>()))
            .Returns((Guid authTokenId) => revokedTokens.Contains(authTokenId));
        var revokedTokenStore = revokedTokenStoreMock.Object;

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(x => x.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        var passwordHasher = passwordHasherMock.Object;
        
        _identityService = new IdentityService(identityConfiguration, _mainDatabase,
            tokenProvider, revokedTokenStore, logger, passwordHasher);

        _user1 = new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = "user1"
        };
        _user2 = new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = "user2"
        };
        _user3 = new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = "user3"
        };
        _mainDatabase.Users.Create(_user1);
        _mainDatabase.Users.Create(_user2);
        _mainDatabase.Users.Create(_user3);
    }

    // TODO: ValidateCredentialsAndCreateTokensPairAsync tests
    
    // TODO: VerifyAuthTokenAndGetPrincipal tests
    
    [Test]
    public async Task RefreshTokensPairAsyncReturnsSuccessAndCreatesNewActiveTokensPairForUser()
    {
        var createTokensResult =
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;
        
        var refreshTokensResult = await _identityService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var newTokensPair = refreshTokensResult.Data;
        var newActiveRefreshToken = (newTokensPair != null
            ? await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
                x.Id.ToString() == newTokensPair.RefreshToken)
            : await Task.FromResult<RefreshTokenEntity?>(null));
        var verificationResult = newTokensPair != null
            ? _identityService.VerifyAuthTokenAndGetPrincipal(newTokensPair.AuthToken)
            : null;
        
        Assert.Multiple(() =>
        {
            Assert.That(refreshTokensResult.IsSucceeded, Is.True);
            Assert.That(newActiveRefreshToken, Is.Not.Null);
            Assert.That(verificationResult, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(newActiveRefreshToken!.IsRevoked, Is.False);
            Assert.That(newActiveRefreshToken.IsUsed, Is.False);
            Assert.That(newActiveRefreshToken.UserId, Is.EqualTo(_user1.Id));
            Assert.That(newActiveRefreshToken.ExpiresDateTimeUtc,
                Is.GreaterThan(DateTime.UtcNow.AddDays(RefreshTokenExpiresDays).AddMinutes(-1)));
            Assert.That(verificationResult!.IsSucceeded, Is.True);
        });
    }

    [Test]
    public async Task RefreshTokensPairAsyncReturnsSuccessAndMarksProvidedActiveRefreshTokenAsUsed()
    {
        var createTokensResult =
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;

        var refreshTokensResult = await _identityService.RefreshTokensPairAsync(tokensPair.RefreshToken);
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
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;
        
        await _identityService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var secondRefreshTokensCallResult = await _identityService.RefreshTokensPairAsync(tokensPair.RefreshToken);

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
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair1 = createTokensResult1.Data!.Tokens;
        var createTokensResult2 =
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair2 = createTokensResult2.Data!.Tokens;
        var createTokensResult3 =
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair3 = createTokensResult3.Data!.Tokens;

        await _identityService.RefreshTokensPairAsync(tokensPair1.RefreshToken);
        await _identityService.RefreshTokensPairAsync(tokensPair1.RefreshToken);
        var providedRefreshToken = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair1.RefreshToken);
        var otherActiveRefreshToken1 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair2.RefreshToken);
        var otherActiveRefreshToken2 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == tokensPair3.RefreshToken);
        var verificationResult1 = _identityService.VerifyAuthTokenAndGetPrincipal(tokensPair1.AuthToken);
        var verificationResult2 = _identityService.VerifyAuthTokenAndGetPrincipal(tokensPair2.AuthToken);
        var verificationResult3 = _identityService.VerifyAuthTokenAndGetPrincipal(tokensPair3.AuthToken);
        
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
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;
        
        // used
        var createOtherTokensResult1 =
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var otherTokensPair1 = createOtherTokensResult1.Data!.Tokens;
        var otherRefreshToken1 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair1.RefreshToken);
        otherRefreshToken1!.IsUsed = true;
        
        // expired
        var createOtherTokensResult2 =
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var otherTokensPair2 = createOtherTokensResult2.Data!.Tokens;
        var otherRefreshToken2 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair2.RefreshToken);
        otherRefreshToken2!.ExpiresDateTimeUtc =
            otherRefreshToken2.ExpiresDateTimeUtc.AddDays(-RefreshTokenExpiresDays);

        await _identityService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        await _identityService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var verificationResult1 = _identityService.VerifyAuthTokenAndGetPrincipal(otherTokensPair1.AuthToken);
        var verificationResult2 = _identityService.VerifyAuthTokenAndGetPrincipal(otherTokensPair2.AuthToken);

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
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user1.GetCredentials());
        var tokensPair = createTokensResult.Data!.Tokens;
        var createOtherTokensResult1 =
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user2.GetCredentials());
        var otherTokensPair1 = createOtherTokensResult1.Data!.Tokens;
        var createOtherTokensResult2 =
            await _identityService.ValidateCredentialsAndCreateTokensPairAsync(_user3.GetCredentials());
        var otherTokensPair2 = createOtherTokensResult2.Data!.Tokens;

        await _identityService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        await _identityService.RefreshTokensPairAsync(tokensPair.RefreshToken);
        var otherActiveRefreshToken1 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair1.RefreshToken);
        var otherActiveRefreshToken2 = await _mainDatabase.RefreshTokens.GetSingleAsync(x =>
            x.Id.ToString() == otherTokensPair2.RefreshToken);
        var verificationResult1 = _identityService.VerifyAuthTokenAndGetPrincipal(otherTokensPair1.AuthToken);
        var verificationResult2= _identityService.VerifyAuthTokenAndGetPrincipal(otherTokensPair2.AuthToken);
        
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

internal static class IdentityServiceTestsHelpers
{
    public static UserCredentialsModel GetCredentials(this UserEntity userEntity)
    {
        return new UserCredentialsModel
        {
            Username = userEntity.Username,
            Password = "password"
        };
    }
}