
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mio.Collections.HashList;

public interface IHashListIndexer<TElement>
{
    public bool Add(TElement element);
    public bool Remove(TElement element);
    public void Clear();
}

public sealed class HashListIndexer<TElement, TKey> : IHashListIndexer<TElement>
{
    private readonly Func<TElement, TKey> _keySelector;
    private readonly Dictionary<TKey, TElement> _indexer;

    public HashListIndexer(IList<TElement> list, Func<TElement, TKey> keySelector)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

        _keySelector = keySelector;
        _indexer = new Dictionary<TKey, TElement>(list.Select(x => KeyValuePair.Create(keySelector(x), x)));
    }

    public bool Add(TElement element)
    {
        var key = _keySelector(element);
        return _indexer.TryAdd(key, element);
    }

    public bool Remove(TElement element)
    {
        var key = _keySelector(element);
        return _indexer.Remove(key);
    }

    public void Clear()
    {
        _indexer.Clear();
    }
}