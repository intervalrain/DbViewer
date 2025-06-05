using System.Diagnostics.CodeAnalysis;

namespace Common.Ddd.Domain.Values;

public sealed class AtomicValueComparer : IEqualityComparer<object?>
{
    public new bool Equals(object? x, object? y)
    {
        if (x is ValueObject voX && y is ValueObject voY)
            return voX.ValueEquals(voY);
        return object.Equals(x, y);
    }

    public int GetHashCode([DisallowNull] object? obj)
    {
        return obj?.GetHashCode() ?? 0;
    }
}