namespace Common.Ddd.Domain.Values;

public abstract class ValueObject
{
    private static readonly IEqualityComparer<object?> _comparer = new AtomicValueComparer();

    protected abstract IEnumerable<object?> GetAtomicValues();

    public bool ValueEquals(object obj)
    {
        if (obj is not ValueObject that || GetType() != obj.GetType())
            return false;

        return GetAtomicValues().SequenceEqual(
            that.GetAtomicValues(), _comparer);
    }
}