using Common.Ddd.Domain.Values;

namespace Common.Test.Ddd.Domain.Values;

public class ValueObjectTests
{
    [Fact]
    public void ValueEquals_ShouldReturnTrue_ForEqualObjects()
    {
        var m1 = new Money(100, "USD");
        var m2 = new Money(100, "USD");

        Assert.True(m1.ValueEquals(m2));
    }

    [Fact]
    public void ValueEquals_ShouldReturnFalse_ForDifferentValues()
    {
        var m1 = new Money(100, "USD");
        var m2 = new Money(200, "USD");

        Assert.False(m1.ValueEquals(m2));
    }

    [Fact]
    public void ValueEquals_ShouldReturnFalse_ForDifferentCurrency()
    {
        var m1 = new Money(100, "USD");
        var m2 = new Money(100, "EUR");

        Assert.False(m1.ValueEquals(m2));
    }

    [Fact]
    public void ValueEquals_ShouldReturnFalse_ForDifferentType()
    {
        var m1 = new Money(100, "USD");

        var other = new Dummy(100, "USD");
        Assert.False(m1.ValueEquals(other));
    }

    private class Dummy : ValueObject
    {
        private readonly decimal _amount;
        private readonly string _currency;

        public Dummy(decimal amount, string currency)
        {
            _amount = amount;
            _currency = currency;
        }

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return _amount;
            yield return _currency;
        }
    }

    private class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
