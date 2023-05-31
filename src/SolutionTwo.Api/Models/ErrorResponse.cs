namespace SolutionTwo.Api.Models;

public class ErrorResponse
{
    public ErrorResponse(string? message, string? requestId = null, string? errorType = null)
    {
        Error = new ErrorData(message, requestId, errorType);
    }
    
    public ErrorResponse(string[] messages, string? requestId = null, string? errorType = null)
    {
        Error = new ErrorData(messages, requestId, errorType);
    }

    public ErrorData Error { get; }

    public class ErrorData
    {
        public ErrorData(string? message, string? requestId = null, string? errorType = null)
        {
            Messages = !string.IsNullOrEmpty(message) ? new[] { message } : Array.Empty<string>();
            RequestId = requestId;
            ErrorType = errorType;
        }
        
        public ErrorData(string[] messages, string? requestId = null, string? errorType = null)
        {
            Messages = messages;
            RequestId = requestId;
            ErrorType = errorType;
        }

        public string[] Messages { get; }

        public string? RequestId { get; }

        public string? ErrorType { get; }
    }
}