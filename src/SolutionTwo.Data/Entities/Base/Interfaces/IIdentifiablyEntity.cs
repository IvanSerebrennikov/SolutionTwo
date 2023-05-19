namespace SolutionTwo.Data.Entities.Base.Interfaces;

public interface IIdentifiablyEntity<TId>
{
    TId Id { get; set; }
}