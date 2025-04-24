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
    private readonly List<IHashListIndexer<TValue>> _indexers; // 索引器

    #region 构造函数
    public HashList()
    {
        _list = new List<TValue>();
        _indexers = new List<IHashListIndexer<TValue>>(1);
    }

    public HashList(int capacity)
    {
        _list = new List<TValue>(capacity);
        _indexers = new List<IHashListIndexer<TValue>>(1);
    }

    public HashList(IEnumerable<TValue> collection)
    {
        _list = new List<TValue>(collection);
        _indexers = new List<IHashListIndexer<TValue>>(1);
    }

    #endregion


    #region 索引实现

    /// <summary>
    /// 创建索引器
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="keySelector"></param>
    /// <exception cref="ArgumentException"></exception>
    public void CreateIndexer<TKey>(Func<TValue, TKey> keySelector)
    {
        foreach (var indexer in _indexers)
        {
            if (indexer is HashListIndexer<TValue, TKey>)
            {
                throw new ArgumentException($"Indexer of {typeof(TKey)} already exists.");
            }
        }
        // keySelector.Method.re

        var newIndexer = new HashListIndexer<TValue, TKey>(_list, keySelector);
        _indexers.Add(newIndexer);
    }

    public void RemoveIndexer<TKey>()
    {
        for (int i = 0; i < _indexers.Count; i++)
        {
            if (_indexers[i] is HashListIndexer<TValue, TKey>)
            {
                _indexers.RemoveAt(i);
                return;
            }
        }
    }

    public void ClearIndexer()
    {
        _indexers.ForEach(static x => x.Clear());
        _indexers.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasIndexer() => _indexers.Count != 0;

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
            foreach (var indexer in _indexers)
                indexer.Remove(oldItem);

            _list[index] = value;
            foreach (var indexer in _indexers)
                indexer.Add(value);
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
        foreach (var indexer in _indexers)
            indexer.Add(item);
    }




    public int IndexOf(TValue item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, TValue item)
    {
        _list.Insert(index, item);
        foreach (var indexer in _indexers)
            indexer.Add(item);
    }

    public void RemoveAt(int index)
    {
        var item = _list[index];
        _list.RemoveAt(index);
        _indexers.ForEach(x => x.Remove(item));
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(TValue item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(TValue[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(TValue item)
    {
        throw new NotImplementedException();
    }

    public bool IsReadOnly => false;

}