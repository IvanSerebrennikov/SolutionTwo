namespace SolutionTwo.Data.Common.Interfaces;

public interface IIdentifiablyEntity<TId>
{
    TId Id { get; set; }
}