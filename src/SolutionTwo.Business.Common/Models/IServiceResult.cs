namespace SolutionTwo.Business.Common.Models;

public interface IServiceResult
{
    bool IsSucceeded { get; }

    string? Message { get; }
    
    string? TraceId { get; }
}

public interface IServiceResult<T> : IServiceResult
{
    T? Data { get; }
}