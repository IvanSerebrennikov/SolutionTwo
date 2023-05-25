namespace SolutionTwo.Domain.Models;

public interface IServiceResult<T>
{
    T? Data { get; }

    bool IsSucceeded { get; }

    string? Message { get; }
}