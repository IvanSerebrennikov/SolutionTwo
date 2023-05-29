using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMainDatabase _mainDatabase;
    private readonly ITokenProvider _tokenProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IdentityConfiguration identityConfiguration,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IMainDatabase mainDatabase, 
        ITokenProvider tokenProvider, 
        IMemoryCache memoryCache,
        ILogger<AuthService> logger)
    {
        _identityConfiguration = identityConfiguration;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _mainDatabase = mainDatabase;
        _tokenProvider = tokenProvider;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<IServiceResult<TokensPairModel>> CreateTokensPairAsync(Guid userId)
    {
        var userEntity = await GetUserWithRolesAsync(userId);
        if (userEntity == null)
            return ServiceResult<TokensPairModel>.Error("User was not found");
        
        var authToken = CreateAuthToken(userEntity, out var authTokenId);
        var refreshToken = CreateRefreshToken(userEntity.Id, authTokenId);
        
        await _mainDatabase.CommitChangesAsync();
        
        var tokensPair = new TokensPairModel(authToken, refreshToken);

        return ServiceResult<TokensPairModel>.Success(tokensPair);
    }

    public async Task<IServiceResult<TokensPairModel>> RefreshTokensPairAsync(Guid refreshToken)
    {
        var refreshTokenEntity = await _refreshTokenRepository.GetByIdAsync(refreshToken);

        string? refreshTokenValidationError = null;
        if (refreshTokenEntity == null)
        {
            _logger.LogWarning(
                $"Attempt to refresh token that doesn't exist. " +
                $"Provided Refresh Token value: {refreshToken}.");
            refreshTokenValidationError = "Provided Refresh token was not found";
        }
        else if (refreshTokenEntity.ExpiresDateTimeUtc <= DateTime.UtcNow)
        {
            refreshTokenValidationError = "Provided Refresh token expired";
        }
        else if (refreshTokenEntity.IsRevoked)
        {
            refreshTokenValidationError = "Provided Refresh token was revoked";
        }
        else if (refreshTokenEntity.IsUsed)
        {
            _logger.LogWarning(
                $"Attempt to refresh already used token. " +
                $"RefreshTokenId: {refreshTokenEntity.Id}, UserId: {refreshTokenEntity.UserId}.");
            await RevokeProvidedAndAllActiveRefreshTokensForUserAsync(refreshTokenEntity.Id, refreshTokenEntity.UserId);
            
            await _mainDatabase.CommitChangesAsync();
            
            refreshTokenValidationError = "Provided Refresh token already used";
        }
        
        if (!string.IsNullOrEmpty(refreshTokenValidationError))
            return ServiceResult<TokensPairModel>.Error(refreshTokenValidationError);

        var userEntity = await GetUserWithRolesAsync(refreshTokenEntity!.UserId);
        if (userEntity == null)
            return ServiceResult<TokensPairModel>.Error("Associated User was not found");
        
        refreshTokenEntity.IsUsed = true;
        
        var authToken = CreateAuthToken(userEntity, out var authTokenId);
        var newRefreshToken = CreateRefreshToken(refreshTokenEntity.UserId, authTokenId);
        
        await _mainDatabase.CommitChangesAsync();
        
        var tokensPair = new TokensPairModel(authToken, newRefreshToken);

        return ServiceResult<TokensPairModel>.Success(tokensPair);
    }

    public IServiceResult<ClaimsPrincipal> ValidateAuthTokenAndGetPrincipal(string tokenString)
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
    
    public bool IsAuthTokenRevoked(Guid authTokenId)
    {
        return _memoryCache.TryGetValue(GetRevokedAuthTokenKey(authTokenId), out int _);
    }

    private async Task<UserEntity?> GetUserWithRolesAsync(Guid userId)
    {
        var userEntity = await _userRepository.GetByIdAsync(
            userId, 
            includeProperties: "Roles",
            asNoTracking: true);

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

        _refreshTokenRepository.Create(refreshToken);

        return refreshToken.Id.ToString();
    }
    
    private async Task RevokeProvidedAndAllActiveRefreshTokensForUserAsync(Guid tokenId, Guid userId)
    {
        var tokenEntities = await _refreshTokenRepository.GetAsync(
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
        _refreshTokenRepository.Update(refreshTokenEntity);
        RevokeAuthToken(refreshTokenEntity.AuthTokenId);
    }
    
    private void RevokeAuthToken(Guid authTokenId)
    {
        _memoryCache.Set(GetRevokedAuthTokenKey(authTokenId), 1,
            TimeSpan.FromMinutes(_identityConfiguration.JwtExpiresMinutes!.Value));
    }

    private static string GetRevokedAuthTokenKey(Guid authTokenId)
    {
        return $"auth-tokens:{authTokenId}:revoked";
    }
}