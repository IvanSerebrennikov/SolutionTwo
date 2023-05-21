using System.Runtime.CompilerServices;
using SolutionTwo.Common.Exceptions;

namespace SolutionTwo.Common.Extensions;

public static class ValueAssertionExtensions
{
    /// <summary>
    ///     Usage: AssertValueIsNotNull(nameof(model.SomeProperty), model.SomeProperty); <br/>
    ///     Result: Assertion failed in SomeMethod. Value of SomeProperty is null.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="caller"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ValueAssertionException"></exception>
    public static void AssertValueIsNotNull<T>(this T value, string name, [CallerMemberName] string caller = "")
    {
        if (value == null) throw new ValueAssertionException(name, caller);
    }
}