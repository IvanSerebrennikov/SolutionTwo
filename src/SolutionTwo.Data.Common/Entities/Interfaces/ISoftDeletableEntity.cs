namespace SolutionTwo.Data.Common.Entities.Interfaces;

public interface ISoftDeletableEntity
{
    bool IsDeleted { get; set; }
}