﻿using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SolutionTwo.Business.Identity.TokenManager.Interfaces;

public interface ITokenManager
{
    string GenerateAuthToken(List<(string, string)> claims, out Guid authTokenId);

    ClaimsPrincipal? ValidateAuthTokenAndGetPrincipal(string tokenString, out SecurityToken? securityToken);
    
    bool IsAuthTokenRevoked(Guid authTokenId);
    
    void RevokeAuthToken(Guid authTokenId);
}