using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
using SolutionTwo.Business.Common.ValueAssertion;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Models.Auth.Incoming;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly IMainDatabase _mainDatabase;
    private readonly ITokenProvider _tokenProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IdentityConfiguration identityConfiguration,
        IMainDatabase mainDatabase, 
        ITokenProvider tokenProvider, 
        IMemoryCache memoryCache,
        ILogger<AuthService> logger, 
        IPasswordHasher passwordHasher)
    {
        _identityConfiguration = identityConfiguration;
        _mainDatabase = mainDatabase;
        _tokenProvider = tokenProvider;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _memoryCache = memoryCache;
    }

    public async Task<IServiceResult<AuthResult>> ValidateCredentialsAndCreateTokensPairAsync(
        UserCredentialsModel userCredentials)
    {
        userCredentials.Username.AssertValueIsNotNull();
        userCredentials.Password.AssertValueIsNotNull();
        
        var userEntity = await VerifyPasswordAndGetUserAsync(userCredentials);
        
        if (userEntity == null)
        {
            return ServiceResult<AuthResult>.Error(
                "User with provided credentials was not found");
        }

        var authToken = CreateAuthToken(userEntity, out var authTokenId);
        var refreshToken = CreateRefreshToken(userEntity.Id, authTokenId);
        
        await _mainDatabase.CommitChangesAsync();
        
        var tokensPair = new TokensPairModel(authToken, refreshToken);
        var authResult = new AuthResult(tokensPair, userEntity.FirstName, userEntity.LastName);

        return ServiceResult<AuthResult>.Success(authResult);
    }

    public async Task<IServiceResult<TokensPairModel>> RefreshTokensPairAsync(string refreshToken)
    {
        if (!Guid.TryParse(refreshToken, out var refreshTokenId))
            return ServiceResult<TokensPairModel>.Error("Invalid refresh token");
        
        var refreshTokenEntity = await _mainDatabase.RefreshTokens.GetByIdAsync(refreshTokenId);

        string? refreshTokenValidationError;
        if (refreshTokenEntity == null)
        {
            _logger.LogWarning(
                $"Attempt to refresh token that doesn't exist. " +
                $"Provided Refresh Token value: {refreshToken}.");
            refreshTokenValidationError = "Provided Refresh token was not found";
        }
        else
        {
            refreshTokenValidationError =
                await ValidateRefreshTokenAndHandlePossibleTokenStealingAsync(refreshTokenEntity);
        }
        
        if (!string.IsNullOrEmpty(refreshTokenValidationError))
            return ServiceResult<TokensPairModel>.Error(refreshTokenValidationError);

        var userEntity = await _mainDatabase.Users.GetByIdAsync(
            refreshTokenEntity!.UserId, 
            includeProperties: "Roles",
            asNoTracking: true);
        if (userEntity == null)
            return ServiceResult<TokensPairModel>.Error("Associated User was not found");
        
        refreshTokenEntity.IsUsed = true;
        
        var authToken = CreateAuthToken(userEntity, out var authTokenId);
        var newRefreshToken = CreateRefreshToken(refreshTokenEntity.UserId, authTokenId);
        
        await _mainDatabase.CommitChangesAsync();
        
        var tokensPair = new TokensPairModel(authToken, newRefreshToken);

        return ServiceResult<TokensPairModel>.Success(tokensPair);
    }

    public IServiceResult<ClaimsPrincipal> VerifyAuthTokenAndGetPrincipal(string tokenString)
    {
        var claimsPrincipal = _tokenProvider.ValidateAuthToken(tokenString, out var securityToken);

        if (claimsPrincipal == null ||
            securityToken == null ||
            !Guid.TryParse(securityToken.Id, out var authTokenId) ||
            IsAuthTokenRevoked(authTokenId))
        {
            return ServiceResult<ClaimsPrincipal>.Error();
        }

        return ServiceResult<ClaimsPrincipal>.Success(claimsPrincipal);
    }
    
    private async Task<UserEntity?> VerifyPasswordAndGetUserAsync(UserCredentialsModel userCredentials)
    {
        var userEntity = await _mainDatabase.Users.GetSingleAsync(
            x => x.Username == userCredentials.Username,
            includeProperties: "Roles", 
            asNoTracking: true);
        
        if (userEntity == null || 
            !_passwordHasher.VerifyHashedPassword(userEntity.PasswordHash, userCredentials.Password))
        {
            var traceId = Guid.NewGuid();
            var incorrectProperty =
                userEntity == null ? nameof(userCredentials.Username) : nameof(userCredentials.Password);
            _logger.LogWarning($"Incorrect {incorrectProperty} was provided during User's credentials verification. " +
                               $"Provided {nameof(userCredentials.Username)}: {userCredentials.Username}. " +
                               $"TraceId: {traceId}.");
        }

        return userEntity;
    }

    private string CreateAuthToken(UserEntity user, out Guid authTokenId)
    {
        var claims = user.Roles.Select(x => (ClaimTypes.Role, x.Name)).ToList();
        claims.Add((ClaimTypes.Name, user.Username));
        var authToken =
            _tokenProvider.GenerateAuthToken(claims, out authTokenId);
        return authToken;
    }
    
    private string CreateRefreshToken(Guid userId, Guid authTokenId)
    {
        var refreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = authTokenId,
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow,
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(_identityConfiguration.RefreshTokenExpiresDays!.Value)
        };

        _mainDatabase.RefreshTokens.Create(refreshToken);

        return refreshToken.Id.ToString();
    }
    
    private async Task<string> ValidateRefreshTokenAndHandlePossibleTokenStealingAsync(RefreshTokenEntity refreshTokenEntity)
    {
        if (refreshTokenEntity.ExpiresDateTimeUtc <= DateTime.UtcNow)
        {
            return "Provided Refresh token expired";
        }
        
        if (refreshTokenEntity.IsRevoked)
        {
            return "Provided Refresh token was revoked";
        }
        
        if (refreshTokenEntity.IsUsed)
        {
            _logger.LogWarning(
                $"Attempt to refresh already used token. " +
                $"RefreshTokenId: {refreshTokenEntity.Id}, UserId: {refreshTokenEntity.UserId}.");
            await RevokeProvidedAndAllActiveRefreshTokensForUserAsync(refreshTokenEntity.Id, refreshTokenEntity.UserId);
            
            await _mainDatabase.CommitChangesAsync();
            
            return "Provided Refresh token already used";
        }

        return "";
    }
    
    private async Task RevokeProvidedAndAllActiveRefreshTokensForUserAsync(Guid tokenId, Guid userId)
    {
        var tokenEntities = await _mainDatabase.RefreshTokens.GetAsync(
            x => x.Id == tokenId || (
                x.UserId == userId && !x.IsUsed && x.ExpiresDateTimeUtc > DateTime.UtcNow));

        foreach (var tokenEntity in tokenEntities)
        {
            RevokeTokens(tokenEntity);
        }
    }

    private void RevokeTokens(RefreshTokenEntity refreshTokenEntity)
    {
        refreshTokenEntity.IsRevoked = true;
        _mainDatabase.RefreshTokens.Update(refreshTokenEntity);
        RevokeAuthToken(refreshTokenEntity.AuthTokenId);
    }
    
    private void RevokeAuthToken(Guid authTokenId)
    {
        _memoryCache.Set(GetRevokedAuthTokenKey(authTokenId), 1,
            TimeSpan.FromMinutes(_identityConfiguration.JwtExpiresMinutes!.Value));
    }
    
    private bool IsAuthTokenRevoked(Guid authTokenId)
    {
        return _memoryCache.TryGetValue(GetRevokedAuthTokenKey(authTokenId), out int _);
    }

    private static string GetRevokedAuthTokenKey(Guid authTokenId)
    {
        return $"auth-tokens:{authTokenId}:revoked";
    }
}