namespace SolutionTwo.Data.Common.Features.OptimisticConcurrency;

public interface IConcurrencyVersionedEntity
{
    /// <summary>
    /// Implemented property should be decorated with [ConcurrencyCheck] attribute
    /// </summary>
    Guid ConcurrencyVersion { get; set; }
}