
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mio.Collections.HashList;

public interface IHashListIndexer<TElement>
{
    public bool Add(TElement element);

    /// <summary>
    /// 添加指定集合的全部元素,如果有重复元素则跳过
    /// </summary>
    /// <param name="elements"></param>
    /// <returns>所有元素都成功添加才返回true</returns>
    public bool AddRange(IEnumerable<TElement> elements);

    /// <summary>
    /// 按元素的key移除元素,该元素可以不是list里的item,只要对应的key存在就会移除
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool Remove(TElement element);
    // public bool RemoveRange(IEnumerable(TElement))
    public void Clear();
}

public sealed class HashListIndexer<TElement, TKey> : IHashListIndexer<TElement>
{
    private readonly Func<TElement, TKey> _keySelector;
    private readonly Dictionary<TKey, TElement> _indexer;
    private readonly IList<TElement> _list;

    public HashListIndexer(Func<TElement, TKey> keySelector)
    {

    }

    /// <summary>
    /// 通过外部列表构建索引器
    /// </summary>
    /// <param name="list"></param>
    /// <param name="keySelector"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public HashListIndexer(IList<TElement> list, Func<TElement, TKey> keySelector)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

        _list = list;
        _keySelector = keySelector;
        _indexer = new Dictionary<TKey, TElement>(list.Select(x => KeyValuePair.Create(keySelector(x), x)));
    }

    public TElement this[TKey key] => _indexer[key];
    public int Count => _indexer.Count;

    public bool Add(TElement element)
    {
        if (element == null) throw new ArgumentNullException(nameof(element));

        var key = _keySelector(element);
        if (_indexer.TryAdd(key, element))
        {
            _list.Add(element);
            return true;
        }
        else
            return false;
    }

    public bool AddRange(IEnumerable<TElement> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));

        bool result = true;
        foreach (var element in collection)
            result &= Add(element);
        return result;
    }

    public bool Remove(TElement element)
    {
        if (element == null) throw new ArgumentNullException(nameof(element));

        var key = _keySelector(element);
        if (_indexer.Remove(key, out element)) // 不是list里的item,也会被移除
        {
            _list.Remove(element);
            return true;
        }
        else
            return false;
    }

    public bool Remove(TKey key)
    {
        if (_indexer.Remove(key, out var element))
        {
            _list.Remove(element);
            return true;
        }
        else
            return false;
    }

    public bool RemoveAt(int index)
    {
        if (index < 0 || index >= _list.Count) throw new ArgumentOutOfRangeException(nameof(index));

        var element = _list[index];
        return Remove(element);
    }

    public void RemoveRange(int index, int count)
    {
        if (index < 0 || index >= _list.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (count < 0 || index + count > _list.Count) throw new ArgumentOutOfRangeException(nameof(count));

        for (int i = index; i < index + count; i++)
        {
            var element = _list[i];
            Remove(element);
        }
    }

    public void RemoveAll(Predicate<TElement> match)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));

        for (int i = 0; i < _list.Count; i++)
        {
            var element = _list[i];
            if (match(element))
                Remove(element);
        }
    }

    public bool Insert(int index, TElement element)
    {
        if (index < 0 || index >= _list.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (element == null) throw new ArgumentNullException(nameof(element));

        var key = _keySelector(element);
        if (_indexer.TryAdd(key, element))
        {
            _list.Insert(index, element);
            return true;
        }
        else
            return false;
    }

    public bool InsertRange(int index, IEnumerable<TElement> collection)
    {
        if (index < 0 || index > _list.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (collection == null) throw new ArgumentNullException(nameof(collection));

        bool result = true;
        foreach (var element in collection)
        {
            var r = Insert(index, element);
            result &= r;
            if (r)
                index++;
        }

        return result;
    }

    public bool Contains(TElement element)
    {
        if (element == null) throw new ArgumentNullException(nameof(element));

        var key = _keySelector(element);
        return _indexer.ContainsKey(key);
    }

    public void Clear()
    {
        _indexer.Clear();
    }
}