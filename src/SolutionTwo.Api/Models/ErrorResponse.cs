namespace SolutionTwo.Api.Models;

public class ErrorResponse
{
    public ErrorResponse(string? message, Guid? traceId = null, string? errorType = null)
    {
        Error = new ErrorData(message, traceId, errorType);
    }

    public ErrorData Error { get; }

    public class ErrorData
    {
        public ErrorData(string? message, Guid? traceId = null, string? errorType = null)
        {
            Message = message;
            TraceId = traceId;
            ErrorType = errorType;
        }

        public string? Message { get; }

        public Guid? TraceId { get; }

        public string? ErrorType { get; }
    }
}