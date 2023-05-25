namespace SolutionTwo.Api.Models;

public class ErrorResponse
{
    public class ErrorData
    {
        public string? Message { get; }
        
        public Guid? ErrorId { get; }
        
        public string? ErrorType { get; }

        public ErrorData(string? message, Guid? errorId = null, string? errorType = null)
        {
            Message = message;
            ErrorId = errorId;
            ErrorType = errorType;
        }
    }

    public ErrorData Error { get; }

    public ErrorResponse(string? message, Guid? errorId = null, string? errorType = null)
    {
        Error = new ErrorData(message, errorId, errorType);
    }
}

