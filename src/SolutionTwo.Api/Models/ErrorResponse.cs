namespace SolutionTwo.Api.Models;

public class ErrorResponse
{
    public class ErrorData
    {
        public string Message { get; private set; }
        
        public Guid? ErrorId { get; private set; }
        
        public string? ErrorType { get; private set; }

        public ErrorData(string message, Guid? errorId = null, string? errorType = null)
        {
            Message = message;
            ErrorId = errorId;
            ErrorType = errorType;
        }
    }

    public ErrorData Error { get; private set; }

    public ErrorResponse(string message, Guid? errorId = null, string? errorType = null)
    {
        Error = new ErrorData(message, errorId, errorType);
    }
}

