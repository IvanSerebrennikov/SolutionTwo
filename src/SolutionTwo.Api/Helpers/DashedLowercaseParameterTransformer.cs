using System.Text.RegularExpressions;

namespace SolutionTwo.Api.Helpers;

public class DashedLowercaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value == null ? null : Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}