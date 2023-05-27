namespace SolutionTwo.Data.Common.Entities.Interfaces;

public interface IIdentifiablyEntity<TId>
{
    TId Id { get; set; }
}