namespace SolutionTwo.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RoleBasedAuthorizeAttribute : Attribute
{
    public RoleBasedAuthorizeAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public string[] Roles { get; }
}