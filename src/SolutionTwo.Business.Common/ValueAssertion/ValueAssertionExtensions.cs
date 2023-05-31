using System.Runtime.CompilerServices;

namespace SolutionTwo.Business.Common.ValueAssertion;

public static class ValueAssertionExtensions
{
    /// <summary>
    ///     Usage: model.SomeProperty.AssertValueIsNotNull(); <br />
    ///     If value is null <see cref="ValueAssertionException"/> throws with message:
    ///     'Assertion failed in SomeMethod. Value of model.SomeProperty is null.'
    /// </summary>
    /// <param name="callerExp"></param>
    /// <param name="value"></param>
    /// <param name="caller"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ValueAssertionException"></exception>
    public static void AssertValueIsNotNull<T>(this T value, 
        [CallerArgumentExpression("value")] string callerExp = "",
        [CallerMemberName] string caller = "")
    {
        if (value == null) throw new ValueAssertionException(callerExp, caller);
    }
}