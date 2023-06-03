using System.Security.Claims;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Common.MultiTenancy;

namespace SolutionTwo.Api.Controllers.Base;

public class ApiAuthorizedControllerBase : ApiControllerBase
{
    protected string CurrentUserUsername
    {
        get
        {
            var claimsValue = GetClaimValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(claimsValue)) 
                return claimsValue;
            
            throw new ApplicationException("Username doesn't exist in Claims");
        }
    }

    protected Guid CurrentUserTenantId
    {
        get
        {
            var claimsValue = GetClaimValue(MultiTenancyClaimNames.TenantId);
            if (!string.IsNullOrEmpty(claimsValue) && Guid.TryParse(claimsValue, out var tenantId)) 
                return tenantId;

            if (HttpContext.User.IsInRole(UserRoles.SuperAdmin))
                return Guid.Empty;
            
            throw new ApplicationException("TenantId doesn't exist in Claims");
        }
    }

    private string? GetClaimValue(string claimType)
    {
        return HttpContext.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }
}