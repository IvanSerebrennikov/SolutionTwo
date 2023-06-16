using System.Security.Claims;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Common.Constants;

namespace SolutionTwo.Api.Controllers.Base;

public class ApiAuthorizedControllerBase : ApiControllerBase
{
    protected string CurrentUserUsername
    {
        get
        {
            var claimValue = GetClaimValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(claimValue)) 
                return claimValue;
            
            throw new ApplicationException("Username doesn't exist in Claims");
        }
    }

    protected Guid CurrentUserTenantId
    {
        get
        {
            var claimValue = GetClaimValue(SolutionTwoClaimNames.TenantId);
            if (!string.IsNullOrEmpty(claimValue) && Guid.TryParse(claimValue, out var tenantId)) 
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