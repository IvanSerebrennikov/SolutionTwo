using System.Security.Claims;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Common.Constants;

namespace SolutionTwo.Api.Controllers.Base;

public class ApiAuthorizedControllerBase : ApiControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var claimValue = GetClaimValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(claimValue) && Guid.TryParse(claimValue, out var userId)) 
                return userId;
            
            throw new ApplicationException("UserId doesn't exist in Claims");
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