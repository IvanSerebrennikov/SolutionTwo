using Microsoft.Extensions.Caching.Memory;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.TokenStore.Interfaces;

namespace SolutionTwo.Business.Identity.TokenStore;

public class RevokedTokenMemoryCacheStore : IRevokedTokenStore
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly IMemoryCache _memoryCache;

    public RevokedTokenMemoryCacheStore(IdentityConfiguration identityConfiguration, IMemoryCache memoryCache)
    {
        _identityConfiguration = identityConfiguration;
        _memoryCache = memoryCache;
    }

    public void RevokeAuthToken(Guid authTokenId)
    {
        _memoryCache.Set(GetRevokedAuthTokenKey(authTokenId), 1,
            TimeSpan.FromMinutes(_identityConfiguration.JwtExpiresMinutes!.Value));
    }
    
    public bool IsAuthTokenRevoked(Guid authTokenId)
    {
        return _memoryCache.TryGetValue(GetRevokedAuthTokenKey(authTokenId), out int _);
    }

    private static string GetRevokedAuthTokenKey(Guid authTokenId)
    {
        return $"auth-tokens:{authTokenId}:revoked";
    }
}