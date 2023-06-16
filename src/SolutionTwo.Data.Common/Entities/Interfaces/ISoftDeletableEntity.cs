namespace SolutionTwo.Data.Common.Entities.Interfaces;

public interface ISoftDeletableEntity
{
    DateTime? DeletedDateTimeUtc { get; set; }
    
    Guid? DeletedBy { get; set; }
}