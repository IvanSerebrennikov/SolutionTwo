namespace SolutionTwo.Common.Exceptions;

public class ConfigurationVerificationException : Exception
{
    public ConfigurationVerificationException(string sectionName) : base($"Section {sectionName} was not found")
    {
        
    }
    
    public ConfigurationVerificationException(string sectionName, IEnumerable<string> invalidProperties) : base(
        $"Next properties from {sectionName} section has empty or incorrect value: {string.Join(", ", invalidProperties)}")
    {
    }
}