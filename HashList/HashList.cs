using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mio.Collections.HashList;

namespace Mio.Collections;

public interface IHashList<T> : IList<T>, IReadOnlyList<T>
{
    // TValue FindByHash(Tkey key);
}


/// <summary>
/// 和<see cref="List{T}"/>一样的顺序列表,但是可以通过添加索引器来加快查找(O(n) -> O(1)). 
/// 支持多个不同类型主键的索引器 
/// </summary>
/// <typeparam name="T"></typeparam>
public class HashList<T> : IHashList<T>
{
    private readonly List<T> _list;  // 容器
    private readonly List<IHashListIndexer<T>> _indexers; // 索引器

    #region 构造函数
    public HashList()
    {
        _list = new List<T>();
        _indexers = new List<IHashListIndexer<T>>(1);
    }

    public HashList(int capacity)
    {
        _list = new List<T>(capacity);
        _indexers = new List<IHashListIndexer<T>>(1);
    }

    public HashList(IEnumerable<T> collection)
    {
        _list = new List<T>(collection);
        _indexers = new List<IHashListIndexer<T>>(1);
    }

    #endregion


    #region 索引实现

    /// <summary>
    /// 创建索引器
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="keySelector"></param>
    /// <exception cref="ArgumentException"></exception>
    public void CreateIndexer<TKey>(Func<T, TKey> keySelector)
    {
        foreach (var indexer in _indexers)
        {
            if (indexer is HashListIndexer<T, TKey>)
            {
                throw new ArgumentException($"Indexer of {typeof(TKey)} already exists.");
            }            
        }
        // keySelector.Method.re

        var newIndexer = new HashListIndexer<T, TKey>(_list, keySelector);
        _indexers.Add(newIndexer);
    }

    public void RemoveIndexer<TKey>()
    {
        for (int i = 0; i < _indexers.Count; i++)
        {
            if (_indexers[i] is HashListIndexer<T, TKey>)
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

    public IEnumerator<T> GetEnumerator()
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
    public T this[int index]
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

    public void Add(T item)
    {
        _list.Add(item);
        foreach (var indexer in _indexers)
            indexer.Add(item);
    }




    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
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

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public bool IsReadOnly => false;

}