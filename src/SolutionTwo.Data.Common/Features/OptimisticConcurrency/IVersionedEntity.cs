namespace SolutionTwo.Data.Common.Features.OptimisticConcurrency;

public interface IVersionedEntity
{
    Guid Version { get; set; }
}