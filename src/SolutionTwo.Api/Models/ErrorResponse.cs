namespace SolutionTwo.Api.Models;

public class ErrorResponse
{
    public ErrorResponse(string? message, Guid? errorId = null, string? errorType = null)
    {
        Error = new ErrorData(message, errorId, errorType);
    }

    public ErrorData Error { get; }

    public class ErrorData
    {
        public ErrorData(string? message, Guid? errorId = null, string? errorType = null)
        {
            Message = message;
            ErrorId = errorId;
            ErrorType = errorType;
        }

        public string? Message { get; }

        public Guid? ErrorId { get; }

        public string? ErrorType { get; }
    }
}