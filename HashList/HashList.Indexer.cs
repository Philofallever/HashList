
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mio.Collections.HashList;

public interface IHashListIndexer<TElement>
{
    public bool Add(TElement element);
    public bool AddRange(IEnumerable<TElement> elements);
    public bool Remove(TElement element);
    // public bool RemoveRange(IEnumerable(TElement))
    public void Clear();
}

public sealed class HashListIndexer<TElement, TKey> : IHashListIndexer<TElement>
{
    private readonly Func<TElement, TKey> _keySelector;
    private readonly Dictionary<TKey, TElement> _indexer;
    private readonly List<TElement> _list;

    public HashListIndexer(List<TElement> list, Func<TElement, TKey> keySelector)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

        _list = list;
        _keySelector = keySelector;
        _indexer = new Dictionary<TKey, TElement>(list.Select(x => KeyValuePair.Create(keySelector(x), x)));
    }

    public TElement this[TKey key]
    {
        get
        {
            if (_indexer.TryGetValue(key, out var result))
                return result;
            return default;
        }
        set
        {
            if (_indexer.TryGetValue(key, out var result))
                _list[_list.IndexOf(result)] = value;
            else
                _list.Add(value);
            _indexer[key] = value;
        }
    }

    public int Count => _indexer.Count;

    /// <summary>
    /// 检查索引器是否有效,即索引器和列表的元素是否一致
    /// </summary>
    public bool IsValid()
    {
        if (_list.Count != _indexer.Count)
            return false;

        foreach (var item in _list)
        {
            var key = _keySelector(item);
            if (!_indexer.TryGetValue(key, out var value) || !EqualityComparer<TElement>.Default.Equals(value, item))
                return false;
        }
        return true;
    }

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

    /// <summary>
    /// 通过外部列表构建索引器
    /// </summary>
    /// <remarks>注意这样做不能再通过列表接口去增删元素</remarks>
    /// <param name="list"></param>
    /// <param name="keySelector"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public bool AddRange(IEnumerable<TElement> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));

        bool result = true;
        foreach (var element in collection)
            result &= Add(element);
        return result;
    }

    /// <summary>
    /// 按元素的key移除元素,该元素可以不是list里的item,只要对应的key存在就会移除
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool Remove(TElement element)
    {
        if (element == null) throw new ArgumentNullException(nameof(element));

        var key = _keySelector(element); // 不是list里的item,也会被移除
        return Remove(key); // WARNING 遍历消耗 见下面
    }

    /// <summary>
    /// 按key移除元素
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(TKey key)
    {
        if (_indexer.Remove(key, out var element))
        {
            _list.Remove(element); // WARNING 这儿有遍历消耗
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// 按索引移除元素
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _list.Count) throw new ArgumentOutOfRangeException(nameof(index));
        var element = _list[index];
        _list.RemoveAt(index);
        _indexer.Remove(_keySelector(element));
    }

    public void RemoveRange(int index, int count)
    {
        if (index < 0) throw new IndexOutOfRangeException(nameof(index));
        if (count < 0) throw new IndexOutOfRangeException(nameof(count));
        if (_list.Count - index < count) throw new IndexOutOfRangeException($"Remove index count out of range index:{index} count:{count}");

        for (int i = index + count - 1; i >= index; i--)
        {
            var element = _list[i];
            _indexer.Remove(_keySelector(element));
        }
        _list.RemoveRange(index, count);
    }

    public void RemoveAll(Predicate<TElement> match)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));

        for (int i = _list.Count - 1; i >= 0; i--)
        {
            var element = _list[i];
            if (match(element))
            {
                _list.RemoveAt(i);
                _indexer.Remove(_keySelector(element));
            }
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

    public bool Contains(TElement item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        var key = _keySelector(item);
        return _indexer.TryGetValue(key, out var e) && EqualityComparer<TElement>.Default.Equals(e, item);
    }

    public void Clear()
    {
        _list.Clear();
        _indexer.Clear();
    }
}