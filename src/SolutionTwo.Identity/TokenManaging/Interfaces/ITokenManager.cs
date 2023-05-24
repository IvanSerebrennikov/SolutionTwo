﻿namespace SolutionTwo.Identity.TokenManaging.Interfaces;

public interface ITokenManager
{
    string GenerateAuthToken(List<(string, string)> claims, out Guid authTokenId);

    bool IsTokenDeactivated(Guid authTokenId);
    
    void DeactivateToken(Guid authTokenId);
}