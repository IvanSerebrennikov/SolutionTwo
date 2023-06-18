namespace SolutionTwo.Data.Common.Features.OptimisticConcurrency;

[AttributeUsage(AttributeTargets.Property)]
public class VersionChangedOnUpdateAttribute : Attribute
{
}