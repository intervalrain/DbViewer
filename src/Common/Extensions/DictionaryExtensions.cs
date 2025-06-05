using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Common.Extensions;

public static class DictionaryExtensions
{
    internal static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T? value)
    {
        if (dictionary.TryGetValue(key, out object? obj) && obj is T result)
        {
            value = result;
            return true;
        }

        value = default;
        return false;
    }

    public static TValue? GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default) where TKey : notnull
    {
        return dictionary.TryGetValue(key, out TValue? obj) ? obj : defaultValue;
    }

    public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default) where TKey : notnull
    {
        return dictionary.TryGetValue(key, out TValue? obj) ? obj : defaultValue;
    }

    public static TValue? GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default) where TKey : notnull
    {
        return dictionary.TryGetValue(key, out TValue? obj) ? obj : defaultValue;
    }

    public static TValue? GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default) where TKey : notnull
    {
        return dictionary.TryGetValue(key, out TValue? obj) ? obj : defaultValue;
    }
}