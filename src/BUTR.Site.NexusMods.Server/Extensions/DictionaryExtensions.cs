using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class DictionaryExtensions
{
    public static TDictionary AddAndReturn<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue value) where TDictionary : IDictionary<TKey, TValue>
    {
        dictionary.Add(key, value);
        return dictionary;
    }

    public static TDictionary SetAndReturn<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue value) where TDictionary : IDictionary<TKey, TValue>
    {
        dictionary[key] = value;
        return dictionary;
    }

    public static TDictionary RemoveAndReturn<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key) where TDictionary : IDictionary<TKey, TValue>
    {
        dictionary.Remove(key);
        return dictionary;
    }
}