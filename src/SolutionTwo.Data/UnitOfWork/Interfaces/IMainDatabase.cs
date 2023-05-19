namespace SolutionTwo.Data.UnitOfWork.Interfaces;

public interface IMainDatabase
{
    Task CommitChangesAsync();

    void CommitChanges();
}