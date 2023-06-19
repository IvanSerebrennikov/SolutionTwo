namespace SolutionTwo.Data.Common.Features.OptimisticConcurrency;

[AttributeUsage(AttributeTargets.Property)]
public class ChangeConcurrencyVersionOnUpdateAttribute : Attribute
{
}