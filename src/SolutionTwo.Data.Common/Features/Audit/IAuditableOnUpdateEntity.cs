namespace SolutionTwo.Data.Common.Features.Audit;

public interface IAuditableOnUpdateEntity
{
    DateTime? LastModifiedDateTimeUtc { get; set; }
    
    Guid? LastModifiedBy { get; set; }
}