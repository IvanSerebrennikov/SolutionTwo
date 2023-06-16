using SolutionTwo.Common.LoggedInUserAccessor.Interfaces;

namespace SolutionTwo.Common.LoggedInUserAccessor;

public class LoggedInUserAccessor : ILoggedInUserGetter, ILoggedInUserSetter
{
    public Guid? UserId { get; private set; }
    
    public void SetLoggedInUserId(Guid userId)
    {
        UserId = userId;
    }
}