namespace StencilPad.Tests.Collections;

using StencilPad.Collections;
using NUnit.Framework;

[TestFixture]
public class FlatSetTests
{
    [Test]
    public void Add_NewElement_ReturnsTrueAndMaintainsSort()
    {
        var set = new FlatSet<int>(4);
        
        Assert.Multiple(() =>
        {
            Assert.That(set.Add(10), Is.True);
            Assert.That(set.Add(5), Is.True);
            Assert.That(set.Add(15), Is.True);
            Assert.That(set.Count, Is.EqualTo(3));
        });

        Assert.Multiple(() =>
        {
            Assert.That(set[0], Is.EqualTo(5));
            Assert.That(set[1], Is.EqualTo(10));
            Assert.That(set[2], Is.EqualTo(15));
        });
    }

    [Test]
    public void Add_DuplicateElement_ReturnsFalse()
    {
        var set = new FlatSet<int>(4);
        set.Add(10);
        
        Assert.Multiple(() =>
        {
            Assert.That(set.Add(10), Is.False);
            Assert.That(set.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void Add_ResizesWhenFull()
    {
        var set = new FlatSet<int>(2);
        set.Add(10);
        set.Add(20);
        
        Assert.That(set.Add(30), Is.True);
        Assert.That(set.Count, Is.EqualTo(3));
        Assert.That(set[2], Is.EqualTo(30));
    }

    [Test]
    public void Remove_ExistingElement_ReturnsTrueAndMaintainsSort()
    {
        var set = new FlatSet<int>();
        set.Add(10);
        set.Add(5);
        set.Add(15);
        
        Assert.Multiple(() =>
        {
            Assert.That(set.Remove(10), Is.True);
            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set[0], Is.EqualTo(5));
            Assert.That(set[1], Is.EqualTo(15));
        });
    }

    [Test]
    public void Remove_NonExistingElement_ReturnsFalse()
    {
        var set = new FlatSet<int>();
        set.Add(10);
        
        Assert.That(set.Remove(5), Is.False);
        Assert.That(set.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveAt_ValidIndex_RemovesCorrectElement()
    {
        var set = new FlatSet<int>();
        set.Add(10);
        set.Add(5);
        set.Add(15);
        
        set.RemoveAt(1); // Removes 10
        
        Assert.Multiple(() =>
        {
            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set[0], Is.EqualTo(5));
            Assert.That(set[1], Is.EqualTo(15));
        });
    }

    [Test]
    public void AddRange_AddsAllUniqueElementsAndMaintainsSort()
    {
        var set = new FlatSet<int>();
        set.Add(10);
        
        var elementsToAdd = new[] { 15, 5, 10, 20, 5 };
        set.AddRange(elementsToAdd);
        
        Assert.Multiple(() =>
        {
            Assert.That(set.Count, Is.EqualTo(4)); // 5, 10, 15, 20
            Assert.That(set[0], Is.EqualTo(5));
            Assert.That(set[1], Is.EqualTo(10));
            Assert.That(set[2], Is.EqualTo(15));
            Assert.That(set[3], Is.EqualTo(20));
        });
    }

    [Test]
    public void Contains_ReturnsCorrectValue()
    {
        var set = new FlatSet<int>();
        set.Add(10);
        
        Assert.Multiple(() =>
        {
            Assert.That(set.Contains(10), Is.True);
            Assert.That(set.Contains(5), Is.False);
        });
    }

    [Test]
    public void GetEnumerator_YieldsSortedElements()
    {
        var set = new FlatSet<int>();
        set.AddRange(new[] { 30, 10, 20 });
        
        var results = new List<int>();
        foreach (var item in set)
        {
            results.Add(item);
        }
        
        Assert.That(results, Is.EqualTo(new[] { 10, 20, 30 }));
    }

    [Test]
    public void Clear_RemovesAllElements()
    {
        var set = new FlatSet<int>();
        set.Add(10);
        set.Add(20);
        
        set.Clear();
        
        Assert.That(set.Count, Is.EqualTo(0));
        Assert.That(set.Contains(10), Is.False);
    }
}
