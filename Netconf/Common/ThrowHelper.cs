using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Netconf;

internal static class ThrowHelper
{
    
    public static void ThrowArgumentNullIfNull<T>([NotNull] T value, [CallerArgumentExpression(nameof(value))] string argumentName = "")
    {
        if (value is null)
        {
            ThrowArgumentNull(argumentName);
        }
    }

    [DoesNotReturn]
    private static void ThrowArgumentNull(string name) => throw new ArgumentNullException(name);
}