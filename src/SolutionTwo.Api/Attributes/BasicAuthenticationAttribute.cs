namespace SolutionTwo.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class BasicAuthenticationAttribute : Attribute
{
    
}