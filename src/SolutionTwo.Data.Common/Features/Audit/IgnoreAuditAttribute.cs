namespace SolutionTwo.Data.Common.Features.Audit;

[AttributeUsage(AttributeTargets.Property)]
public class IgnoreAuditAttribute : Attribute
{
}