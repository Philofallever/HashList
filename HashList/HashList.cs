using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mio.Collections.HashList;

namespace Mio.Collections;

public interface IHashList<TKey, TValue> : IList<TValue>, IReadOnlyList<TValue>
{
    // TValue FindByHash(Tkey key);
}

/// <summary>
/// 和<see cref="List{T}"/>一样的顺序列表,但是可以通过添加索引器来加快查找(O(n) -> O(1)). 
/// 支持多个不同类型主键的索引器 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed class HashList<TKey, TValue> : IHashList<TKey, TValue>
{
    private readonly List<TValue> _list;  // 容器
    private readonly HashListIndexer<TValue, TKey> _indexer; // 索引器

    #region 构造函数
    public HashList(Func<TValue, TKey> keySelector)
    {
        _list = new List<TValue>();        
        _indexer = new HashListIndexer<TValue, TKey>(_list, keySelector);    
    }

    public HashList(int capacity, Func<TValue, TKey> keySelector)
    {
        _list = new List<TValue>(capacity);
        _indexer = new HashListIndexer<TValue, TKey>(_list, keySelector);
    }

    public HashList(IEnumerable<TValue> collection, Func<TValue, TKey> keySelector)
    {
        _list = new List<TValue>(collection);
        _indexer = new HashListIndexer<TValue, TKey>(_list, keySelector);
    }

    #endregion

    #region 索引实现

    public void ClearIndexer()
    {
        _indexer.Clear();
    }

    public bool HasIndexer() => _indexer != null;

    #endregion

    #region 迭代器

    public IEnumerator<TValue> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    /// <summary>
    /// 列表的Index索引
    /// </summary>
    /// <param name="index"></param>
    public TValue this[int index]
    {
        get => _list[index];
        set
        {
            var oldItem = _list[index];
            _indexer.Remove(oldItem);

            _list[index] = value;
            _indexer.Add(value);
        }
    }

    public int Count => _list.Count;

    public int Capacity
    {
        get => _list.Capacity;
        set => _list.Capacity = value;
    }

    public void Add(TValue item)
    {
        _list.Add(item);
        _indexer.Add(item);
    }

    public int IndexOf(TValue item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, TValue item)
    {
        _list.Insert(index, item);
        _indexer.Add(item);
    }

    public void RemoveAt(int index)
    {
        var item = _list[index];
        _list.RemoveAt(index);
        _indexer.Remove(item);
    }

    public void Clear()
    {
        _list.Clear();
        _indexer.Clear();
    }

    public bool Contains(TValue item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(TValue[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public bool Remove(TValue item)
    {
        if (_list.Remove(item))
        {
            _indexer.Remove(item);
            return true;
        }
        return false;
    }

    public bool IsReadOnly => false;

}
