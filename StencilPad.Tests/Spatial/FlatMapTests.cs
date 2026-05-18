namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;
using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class FlatMapTests
{
    [Test]
    public void Add_NewKey_ReturnsTrueAndMaintainsSort()
    {
        var map = new FlatMap<int, string>(4);
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Add(10, "Ten"), Is.True);
            Assert.That(map.Add(5, "Five"), Is.True);
            Assert.That(map.Add(15, "Fifteen"), Is.True);
            Assert.That(map.Count, Is.EqualTo(3));
        });

        Assert.Multiple(() =>
        {
            Assert.That(map[0].Key, Is.EqualTo(5));
            Assert.That(map[0].Value, Is.EqualTo("Five"));
            Assert.That(map[1].Key, Is.EqualTo(10));
            Assert.That(map[1].Value, Is.EqualTo("Ten"));
            Assert.That(map[2].Key, Is.EqualTo(15));
            Assert.That(map[2].Value, Is.EqualTo("Fifteen"));
        });
    }

    [Test]
    public void Add_DuplicateKey_ReturnsFalse()
    {
        var map = new FlatMap<int, string>(4);
        map.Add(10, "Ten");
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Add(10, "Duplicate"), Is.False);
            Assert.That(map.Count, Is.EqualTo(1));
            Assert.That(map[0].Value, Is.EqualTo("Ten"));
        });
    }

    [Test]
    public void Add_ResizesWhenFull()
    {
        var map = new FlatMap<int, string>(2);
        map.Add(10, "Ten");
        map.Add(20, "Twenty");
        
        Assert.That(map.Add(30, "Thirty"), Is.True);
        Assert.That(map.Count, Is.EqualTo(3));
        Assert.That(map[2].Key, Is.EqualTo(30));
    }

    [Test]
    public void Remove_ExistingKey_ReturnsTrueAndMaintainsSort()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");
        map.Add(5, "Five");
        map.Add(15, "Fifteen");
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Remove(10), Is.True);
            Assert.That(map.Count, Is.EqualTo(2));
            Assert.That(map[0].Key, Is.EqualTo(5));
            Assert.That(map[1].Key, Is.EqualTo(15));
        });
    }

    [Test]
    public void Remove_NonExistingKey_ReturnsFalse()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");
        
        Assert.That(map.Remove(5), Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveAt_ValidIndex_RemovesCorrectElement()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");
        map.Add(5, "Five");
        map.Add(15, "Fifteen");
        
        map.RemoveAt(1); // Removes Key 10
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Count, Is.EqualTo(2));
            Assert.That(map[0].Key, Is.EqualTo(5));
            Assert.That(map[1].Key, Is.EqualTo(15));
        });
    }

    [Test]
    public void TryGetValue_ExistingKey_ReturnsTrueAndCorrectValue()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");
        
        Assert.Multiple(() =>
        {
            Assert.That(map.TryGetValue(10, out var value), Is.True);
            Assert.That(value, Is.EqualTo("Ten"));
        });
    }

    [Test]
    public void TryGetValue_NonExistingKey_ReturnsFalseAndDefault()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");
        
        Assert.Multiple(() =>
        {
            Assert.That(map.TryGetValue(5, out var value), Is.False);
            Assert.That(value, Is.Null);
        });
    }

    [Test]
    public void Contains_ReturnsCorrectValue()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Contains(10), Is.True);
            Assert.That(map.Contains(5), Is.False);
        });
    }

    [Test]
    public void GetEnumerator_YieldsSortedElements()
    {
        var map = new FlatMap<int, string>();
        map.Add(30, "Thirty");
        map.Add(10, "Ten");
        map.Add(20, "Twenty");
        
        var results = new List<int>();
        foreach (var kvp in map)
        {
            results.Add(kvp.Key);
        }
        
        Assert.That(results, Is.EqualTo(new[] { 10, 20, 30 }));
    }

    [Test]
    public void Clear_RemovesAllElements()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");
        map.Add(20, "Twenty");
        
        map.Clear();
        
        Assert.That(map.Count, Is.EqualTo(0));
        Assert.That(map.Contains(10), Is.False);
    }
}
