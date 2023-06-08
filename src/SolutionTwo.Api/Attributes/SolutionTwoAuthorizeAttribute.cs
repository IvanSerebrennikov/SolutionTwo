namespace SolutionTwo.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class SolutionTwoAuthorizeAttribute : Attribute
{
    public SolutionTwoAuthorizeAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public string[] Roles { get; }
}