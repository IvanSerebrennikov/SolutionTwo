using System.Security.Claims;
using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Models.Auth.Incoming;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;
using SolutionTwo.Business.Identity.TokenStore.Interfaces;
using SolutionTwo.Common.Constants;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Identity.Services;

public class IdentityService : IIdentityService
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly IMainDatabase _mainDatabase;
    private readonly ITokenProvider _tokenProvider;
    private readonly IDeactivatedTokenStore _deactivatedTokenStore;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        IdentityConfiguration identityConfiguration,
        IMainDatabase mainDatabase, 
        ITokenProvider tokenProvider, 
        IDeactivatedTokenStore deactivatedTokenStore,
        ILogger<IdentityService> logger, 
        IPasswordHasher passwordHasher)
    {
        _identityConfiguration = identityConfiguration;
        _mainDatabase = mainDatabase;
        _tokenProvider = tokenProvider;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _deactivatedTokenStore = deactivatedTokenStore;
    }

    public async Task<IServiceResult<AuthResult>> ValidateCredentialsAndCreateTokensPairAsync(
        UserCredentialsModel userCredentials)
    {
        var userEntity = await GetUserWithRolesByCredentialsAsync(userCredentials);
        
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
            // possible token brute force
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
            include: x => x.Roles);
        if (userEntity == null)
            return ServiceResult<TokensPairModel>.Error("Associated User was not found");
        
        refreshTokenEntity.IsUsed = true;
        _mainDatabase.RefreshTokens.Update(refreshTokenEntity, x => x.IsUsed);
        
        var authToken = CreateAuthToken(userEntity, out var authTokenId);
        var newRefreshToken = CreateRefreshToken(refreshTokenEntity.UserId, authTokenId);
        
        await _mainDatabase.CommitChangesAsync();
        
        var tokensPair = new TokensPairModel(authToken, newRefreshToken);

        return ServiceResult<TokensPairModel>.Success(tokensPair);
    }

    public IServiceResult<ClaimsPrincipal> VerifyAuthTokenAndGetPrincipal(string authToken)
    {
        var claimsPrincipal = _tokenProvider.ValidateAuthToken(authToken, out var authTokenId);

        if (claimsPrincipal == null ||
            authTokenId == null ||
            _deactivatedTokenStore.IsAuthTokenDeactivated(authTokenId.Value))
        {
            return ServiceResult<ClaimsPrincipal>.Error();
        }

        return ServiceResult<ClaimsPrincipal>.Success(claimsPrincipal);
    }

    public async Task ResetUserAccessAsync(Guid userId)
    {
        await SetAllActiveRefreshTokensAsRevokedAndDeactivateRelatedAuthTokensForUserAsync(
            userId);

        await _mainDatabase.CommitChangesAsync();
    }

    private async Task<UserEntity?> GetUserWithRolesByCredentialsAsync(UserCredentialsModel userCredentials)
    {
        var userEntity = await _mainDatabase.Users.GetSingleAsync(
            x => x.Username == userCredentials.Username,
            include: x => x.Roles);
        
        if (userEntity == null || 
            !_passwordHasher.VerifyHashedPassword(userEntity.PasswordHash, userCredentials.Password))
        {
            var incorrectProperty =
                userEntity == null ? nameof(userCredentials.Username) : nameof(userCredentials.Password);
            _logger.LogWarning($"Incorrect {incorrectProperty} was provided during User's credentials verification. " +
                               $"Provided {nameof(userCredentials.Username)}: {userCredentials.Username}.");
        }

        return userEntity;
    }

    private string CreateAuthToken(UserEntity userWithRolesEntity, out Guid authTokenId)
    {
        var claims = userWithRolesEntity.Roles.Select(x => (ClaimTypes.Role, x.Name)).ToList();
        
        claims.Add((ClaimTypes.Name, userWithRolesEntity.Username));
        
        claims.Add((ClaimTypes.NameIdentifier, userWithRolesEntity.Id.ToString()));
        
        claims.Add((SolutionTwoClaimNames.TenantId, userWithRolesEntity.TenantId.ToString()));
        
        var authToken =
            _tokenProvider.GenerateAuthToken(claims, out authTokenId);
        return authToken;
    }
    
    private string CreateRefreshToken(Guid userId, Guid authTokenId)
    {
        var refreshTokenEntity = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = authTokenId,
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow,
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(_identityConfiguration.RefreshTokenExpiresDays!.Value)
        };

        _mainDatabase.RefreshTokens.Create(refreshTokenEntity);

        return refreshTokenEntity.Id.ToString();
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
            // possible token stealing 
            _logger.LogWarning(
                $"Attempt to refresh already used token. " +
                $"RefreshTokenId: {refreshTokenEntity.Id}, UserId: {refreshTokenEntity.UserId}.");
            await SetProvidedAndAllActiveRefreshTokensAsRevokedAndDeactivateRelatedAuthTokensForUserAsync(
                refreshTokenEntity);
            
            await _mainDatabase.CommitChangesAsync();
            
            return "Provided Refresh token already used";
        }

        return "";
    }
    
    private async Task SetProvidedAndAllActiveRefreshTokensAsRevokedAndDeactivateRelatedAuthTokensForUserAsync(
        RefreshTokenEntity providedRefreshTokenEntity)
    {
        SetRefreshTokenAsRevokedAndDeactivateRelatedAuthToken(providedRefreshTokenEntity);

        await SetAllActiveRefreshTokensAsRevokedAndDeactivateRelatedAuthTokensForUserAsync(
            providedRefreshTokenEntity.UserId);
    }
    
    private async Task SetAllActiveRefreshTokensAsRevokedAndDeactivateRelatedAuthTokensForUserAsync(Guid userId)
    {
        var refreshTokenEntities = await _mainDatabase.RefreshTokens.GetAsync(x =>
            x.UserId == userId && !x.IsUsed && x.ExpiresDateTimeUtc > DateTime.UtcNow);

        foreach (var refreshTokenEntity in refreshTokenEntities)
        {
            SetRefreshTokenAsRevokedAndDeactivateRelatedAuthToken(refreshTokenEntity);
        }
    }

    private void SetRefreshTokenAsRevokedAndDeactivateRelatedAuthToken(RefreshTokenEntity refreshTokenEntity)
    {
        refreshTokenEntity.IsRevoked = true;
        _mainDatabase.RefreshTokens.Update(refreshTokenEntity, x => x.IsRevoked);
        _deactivatedTokenStore.DeactivateAuthToken(refreshTokenEntity.AuthTokenId);
    }
}