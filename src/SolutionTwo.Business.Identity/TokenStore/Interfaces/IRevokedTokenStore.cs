namespace SolutionTwo.Business.Identity.TokenStore.Interfaces;

public interface IRevokedTokenStore
{
    void RevokeAuthToken(Guid authTokenId);

    bool IsAuthTokenRevoked(Guid authTokenId);
}