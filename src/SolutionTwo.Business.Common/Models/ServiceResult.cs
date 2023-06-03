namespace SolutionTwo.Business.Common.Models;

public class ServiceResult : IServiceResult
{
    private ServiceResult(string? message, bool isSucceeded)
    {
        Message = message;
        IsSucceeded = isSucceeded;
    }

    public bool IsSucceeded { get; }

    public string? Message { get; }

    public static IServiceResult Success(string? message = null)
    {
        return new ServiceResult(message, true);
    }
    
    public static IServiceResult Error(string? message = null)
    {
        return new ServiceResult(message, false);
    }
}

public class ServiceResult<T> : IServiceResult<T>
{
    private ServiceResult(T? data, string? message, bool isSucceeded)
    {
        Data = data;
        Message = message;
        IsSucceeded = isSucceeded;
    }
    
    public T? Data { get; }

    public bool IsSucceeded { get; }

    public string? Message { get; }

    public static IServiceResult<T> Success(T? data, string? message = null)
    {
        return new ServiceResult<T>(data, message, true);
    }
    
    public static IServiceResult<T> Error(string? message = null)
    {
        return new ServiceResult<T>(default, message, false);
    }
}