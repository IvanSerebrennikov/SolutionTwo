using System.Security.Claims;
using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Common.PasswordManager.Interfaces;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Models.Auth.Incoming;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenManager.Interfaces;
using SolutionTwo.Common.Extensions;
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
    private readonly ITokenManager _tokenManager;
    private readonly IPasswordManager _passwordManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IdentityConfiguration identityConfiguration,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IMainDatabase mainDatabase, 
        ITokenManager tokenManager, 
        IPasswordManager passwordManager, 
        ILogger<AuthService> logger)
    {
        _identityConfiguration = identityConfiguration;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _mainDatabase = mainDatabase;
        _tokenManager = tokenManager;
        _passwordManager = passwordManager;
        _logger = logger;
    }

    public async Task<IServiceResult<TokensPairModel>> CreateTokensPairAsync(UserCredentialsModel userCredentials)
    {
        userCredentials.Username.AssertValueIsNotNull();
        userCredentials.Password.AssertValueIsNotNull();
        
        var userEntity = await _userRepository.GetSingleAsync(
            x => x.Username == userCredentials.Username,
            includeProperties: "Roles", 
            asNoTracking: true);
        
        if (userEntity == null || 
            !_passwordManager.VerifyHashedPassword(userEntity.PasswordHash, userCredentials.Password!))
        {
            return ServiceResult<TokensPairModel>.Error(
                "User with provided credentials was not found");
        }
        
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
            refreshTokenValidationError = "Provided Refresh token was not found";
        else if (refreshTokenEntity.ExpiresDateTimeUtc <= DateTime.UtcNow)
            refreshTokenValidationError = "Provided Refresh token expired";
        else if (refreshTokenEntity.IsRevoked)
            refreshTokenValidationError = "Provided Refresh token was revoked";
        else if (refreshTokenEntity.IsUsed)
        {
            _logger.LogWarning(
                $"Someone is trying to refresh already used token. " +
                $"RefreshTokenId: {refreshTokenEntity.Id}, UserId: {refreshTokenEntity.UserId}.");
            await RevokeProvidedAndAllActiveRefreshTokensForUserAsync(refreshTokenEntity.Id, refreshTokenEntity.UserId);
            
            await _mainDatabase.CommitChangesAsync();
            
            refreshTokenValidationError = "Provided Refresh token already used";
        }
        
        if (!string.IsNullOrEmpty(refreshTokenValidationError))
            return ServiceResult<TokensPairModel>.Error(refreshTokenValidationError);

        var userEntity = await _userRepository.GetByIdAsync(
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

    private string CreateAuthToken(UserEntity user, out Guid authTokenId)
    {
        var claims = user.Roles.Select(x => (ClaimTypes.Role, x.Name)).ToList();
        claims.Add((ClaimTypes.Name, user.Username));
        var authToken =
            _tokenManager.GenerateAuthToken(claims, out authTokenId);
        return authToken;
    }
    
    private void RevokeTokens(RefreshTokenEntity refreshTokenEntity)
    {
        refreshTokenEntity.IsRevoked = true;
        _refreshTokenRepository.Update(refreshTokenEntity);
        _tokenManager.RevokeAuthToken(refreshTokenEntity.AuthTokenId);
    }
}