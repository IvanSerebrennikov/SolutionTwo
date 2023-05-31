namespace SolutionTwo.Api.Models;

public class ErrorResponse
{
    public ErrorResponse(string? message, string? traceId = null, string? errorType = null)
    {
        Error = new ErrorData(message, traceId, errorType);
    }
    
    public ErrorResponse(string[] messages, string? traceId = null, string? errorType = null)
    {
        Error = new ErrorData(messages, traceId, errorType);
    }

    public ErrorData Error { get; }

    public class ErrorData
    {
        public ErrorData(string? message, string? traceId = null, string? errorType = null)
        {
            Messages = !string.IsNullOrEmpty(message) ? new[] { message } : Array.Empty<string>();
            TraceId = traceId;
            ErrorType = errorType;
        }
        
        public ErrorData(string[] messages, string? traceId = null, string? errorType = null)
        {
            Messages = messages;
            TraceId = traceId;
            ErrorType = errorType;
        }

        public string[] Messages { get; }

        public string? TraceId { get; }

        public string? ErrorType { get; }
    }
}