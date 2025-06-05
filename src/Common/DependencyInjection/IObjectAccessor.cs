namespace Common.DependencyInjection;

public interface IObjectAccessor<out T>
{
    T? Value { get; }
}
