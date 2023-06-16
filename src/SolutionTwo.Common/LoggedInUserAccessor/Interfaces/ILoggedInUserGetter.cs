namespace SolutionTwo.Common.LoggedInUserAccessor.Interfaces;

public interface ILoggedInUserGetter
{
    Guid? UserId { get; }
}