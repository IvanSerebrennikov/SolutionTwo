namespace SolutionTwo.Business.Common.ValueAssertion;

public class ValueAssertionException : Exception
{
    public ValueAssertionException(string parameterName, string callerName) : base(
        $"Assertion failed in {callerName}. Value of {parameterName} is null.")
    {
    }
}