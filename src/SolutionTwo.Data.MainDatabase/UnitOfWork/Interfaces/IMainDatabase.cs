namespace SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

public interface IMainDatabase
{
    Task CommitChangesAsync();

    void CommitChanges();
}