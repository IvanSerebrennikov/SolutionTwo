namespace SolutionTwo.Data.Common.Features.SoftDeletion;

public interface ISoftDeletableEntity
{
    DateTime? DeletedDateTimeUtc { get; set; }
    
    Guid? DeletedBy { get; set; }
}