namespace SolutionTwo.Data.Common.Features.OptimisticConcurrency;

public interface IVersionedEntity
{
    Guid ConcurrencyVersion { get; set; }
}