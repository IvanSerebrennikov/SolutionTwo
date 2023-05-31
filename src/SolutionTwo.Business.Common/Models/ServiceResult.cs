namespace SolutionTwo.Business.Common.Models;

public class ServiceResult<T> : IServiceResult<T>
{
    private ServiceResult(T? data, string? message, bool isSucceeded, string? traceId)
    {
        Data = data;
        Message = message;
        IsSucceeded = isSucceeded;
        TraceId = traceId;
    }
    
    public T? Data { get; }

    public bool IsSucceeded { get; }

    public string? Message { get; }

    public string? TraceId { get; }

    public static IServiceResult<T> Success(T? data, string? message = null)
    {
        return new ServiceResult<T>(data, message, true, null);
    }
    
    public static IServiceResult<T> Error(string? message = null, string? traceId = null)
    {
        return new ServiceResult<T>(default, message, false, traceId);
    }
}