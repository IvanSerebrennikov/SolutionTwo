namespace SolutionTwo.Data.Common.Features.Audit;

public interface IAuditableOnCreateEntity
{
    DateTime CreatedDateTimeUtc { get; set; }
    
    Guid CreatedBy { get; set; }
}