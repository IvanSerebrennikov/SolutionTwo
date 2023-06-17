namespace SolutionTwo.Business.Identity.TokenStore.Interfaces;

public interface IDeactivatedTokenStore
{
    void DeactivateAuthToken(Guid authTokenId);

    bool IsAuthTokenDeactivated(Guid authTokenId);
}