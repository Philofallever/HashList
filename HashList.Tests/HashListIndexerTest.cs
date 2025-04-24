using System.Collections.Generic;
using System.Reflection;
using Mio.Collections.HashList;

namespace HashList.Test;


[TestFixture]
public class HashListIndexerTest
{

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int phone { get; set; }
    }

    public List<Person> FivePersonList = new List<Person>(){
        new Person(){Id = 1, Name = "张三", phone = 123451},
        new Person(){Id = 2, Name = "李四", phone = 123452},
        new Person(){Id = 3, Name = "王五", phone = 123453},
        new Person(){Id = 4, Name = "赵六", phone = 123454},
        new Person(){Id = 5, Name = "孙七", phone = 123455},
    };

    [SetUp]
    public void Setup()
    {
    }

    public IList<TValue> GetList<TValue>(IHashListIndexer<TValue> target)
    {
        return target.GetType().GetField("_list", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target) as IList<TValue>;
    }

    public Dictionary<TKey, TValue> GetIndex<TKey, TValue>(IHashListIndexer<TValue> target)
    {
        return target.GetType().GetField("_indexer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target) as Dictionary<TKey, TValue>;
    }

    [Test]
    public void TestConstructorNullArg()
    {
        Assert.That(() => new HashListIndexer<Person, int>(null, x => x.Id), Throws.ArgumentNullException);
        Assert.That(() => new HashListIndexer<Person, int>(FivePersonList, null), Throws.ArgumentNullException);
    }

    [Test]
    public void TestConstructorHasSame()
    {
        var list = new List<Person>(FivePersonList);
        list.AddRange(list);
        Assert.That(() => new HashListIndexer<Person, int>(list, x => x.Id), Throws.ArgumentException);
    }

    [Test]
    public void TestAdd()
    {
        var indexer = new HashListIndexer<Person, int>(FivePersonList, x => x.Id);
        Assert.That(indexer.Add(new Person() { Id = 6, Name = "周八", phone = 123456 }), Is.True);
        Assert.That(indexer.Add(new Person() { Id = 6, Name = "周八", phone = 123456 }), Is.False);
        Assert.That(GetList(indexer), Has.Count.EqualTo(6));
        CheckContainer(indexer);
    }

    [Test]
    public void TestAddRange()
    {
        var indexer = new HashListIndexer<Person, int>(FivePersonList, x => x.Id);
        Assert.That(indexer.AddRange(new List<Person>(){
            new Person(){Id = 6, Name = "周八", phone = 123456},
            new Person(){Id = 7, Name = "吴九", phone = 123456},
        }), Is.True);
        Assert.That(indexer.AddRange(new List<Person>(){
            new Person(){Id = 6, Name = "周八", phone = 123456},
            new Person(){Id = 7, Name = "吴九", phone = 123456},
        }), Is.False);

        Assert.That(GetList(indexer), Has.Count.EqualTo(7));
        CheckContainer(indexer);
    }

    [Test]
    public void TestRemoveItem()
    {
        var indexer = new HashListIndexer<Person, int>(FivePersonList, x => x.Id);
        Assert.That(indexer.Remove(new Person() { Id = 1, Name = "张三", phone = 123456 }), Is.True);
        Assert.That(indexer.Remove(new Person() { Id = 1, Name = "张三", phone = 123456 }), Is.False);
        Assert.That(GetList(indexer), Has.Count.EqualTo(4));
        CheckContainer(indexer);

        var indexer2 = new HashListIndexer<Person, int>(FivePersonList, x => x.Id);
        var p = GetList
        Assert.That(indexer2.Remove(), Is.False);
        Assert.That(GetList(indexer2), Has.Count.EqualTo(5));
        CheckContainer(indexer2);
    }

    [Test]
    public void TestRemoveKey()
    {
        var indexer = new HashListIndexer<Person, int>(FivePersonList, x => x.Id);
        Assert.That(indexer.Remove(1), Is.True);
        Assert.That(indexer.Remove(1), Is.False);
        Assert.That(GetList(indexer), Has.Count.EqualTo(4));
        CheckContainer(indexer);

        var indexer2 = new HashListIndexer<Person, int>(FivePersonList, x => x.Id);
        Assert.That(indexer2.Remove(6), Is.False);
        Assert.That(GetList(indexer2), Has.Count.EqualTo(5));
        CheckContainer(indexer2);
    }

    
    void CheckContainer(HashListIndexer<Person, int> indexer)
    {
        var list = GetList(indexer);
        var dict = GetIndex<int, Person>(indexer);
        Assert.That(list, Has.Count.EqualTo(dict.Count));
        foreach (var p in list)
        {
            TestContext.Out.WriteLine($"{p.Id} {p.Name} {p.phone}");
            Assert.That(dict[p.Id], Is.EqualTo(p));
        }
    }
}
