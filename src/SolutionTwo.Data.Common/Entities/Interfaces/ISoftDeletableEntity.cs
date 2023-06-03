namespace SolutionTwo.Data.Common.Entities.Interfaces;

public interface ISoftDeletableEntity
{
    bool IsDeleted { get; set; } // or DateTime? DeletedDateTime (+ may be DeletedBy)
}