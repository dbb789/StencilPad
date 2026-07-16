namespace StencilPad.Tests.Collections;

using StencilPad.Collections;
using NUnit.Framework;

[TestFixture]
public class FlatMapTests
{
    [Test]
    public void Add_NewElement_ReturnsTrueAndMaintainsSort()
    {
        var map = new FlatMap<int, string>(4);
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Add(10, "Ten"), Is.True);
            Assert.That(map.Add(5, "Five"), Is.True);
            Assert.That(map.Add(15, "Fifteen"), Is.True);
            Assert.That(map.Count, Is.EqualTo(3));
        });

        // Elements should be sorted by key (5, 10, 15)
        var elements = new List<KeyValuePair<int, string>>();
        foreach (var kvp in map)
        {
            elements.Add(kvp);
        }

        Assert.Multiple(() =>
        {
            Assert.That(elements[0].Key, Is.EqualTo(5));
            Assert.That(elements[0].Value, Is.EqualTo("Five"));
            Assert.That(elements[1].Key, Is.EqualTo(10));
            Assert.That(elements[1].Value, Is.EqualTo("Ten"));
            Assert.That(elements[2].Key, Is.EqualTo(15));
            Assert.That(elements[2].Value, Is.EqualTo("Fifteen"));
        });
    }

    [Test]
    public void Add_DuplicateKey_UpdatesValueAndReturnsFalse()
    {
        var map = new FlatMap<int, string>(4);
        map.Add(10, "Ten");
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Add(10, "Ten Updated"), Is.False);
            Assert.That(map.Count, Is.EqualTo(1));
            Assert.That(map[10], Is.EqualTo("Ten Updated"));
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
        Assert.That(map[30], Is.EqualTo("Thirty"));
    }

    [Test]
    public void Remove_ExistingElement_ReturnsTrueAndMaintainsSort()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");
        map.Add(5, "Five");
        map.Add(15, "Fifteen");
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Remove(10), Is.True);
            Assert.That(map.Count, Is.EqualTo(2));
            Assert.That(map.Contains(5), Is.True);
            Assert.That(map.Contains(15), Is.True);
            Assert.That(map.Contains(10), Is.False);
        });
    }

    [Test]
    public void Remove_NonExistingElement_ReturnsFalse()
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
        
        map.RemoveAt(1); // Removes element at index 1 (Key 10)
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Count, Is.EqualTo(2));
            Assert.That(map.Contains(10), Is.False);
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
    public void TryGetValue_ReturnsCorrectValue()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");

        Assert.Multiple(() =>
        {
            Assert.That(map.TryGetValue(10, out var val1), Is.True);
            Assert.That(val1, Is.EqualTo("Ten"));

            Assert.That(map.TryGetValue(5, out var val2), Is.False);
            Assert.That(val2, Is.Null);
        });
    }

    [Test]
    public void Indexer_Get_ExistingKey_ReturnsValue()
    {
        var map = new FlatMap<int, string>();
        map.Add(10, "Ten");

        Assert.That(map[10], Is.EqualTo("Ten"));
    }

    [Test]
    public void Indexer_Get_NonExistingKey_ThrowsKeyNotFoundException()
    {
        var map = new FlatMap<int, string>();

        Assert.Throws<KeyNotFoundException>(() => _ = map[10]);
    }

    [Test]
    public void Indexer_Set_UpdatesOrAddsElement()
    {
        var map = new FlatMap<int, string>();
        
        map[10] = "Ten"; // Add
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map[10], Is.EqualTo("Ten"));

        map[10] = "Ten Updated"; // Update
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map[10], Is.EqualTo("Ten Updated"));
    }

    [Test]
    public void GetEnumerator_YieldsSortedElements()
    {
        var map = new FlatMap<int, string>();
        map.Add(30, "Thirty");
        map.Add(10, "Ten");
        map.Add(20, "Twenty");
        
        var results = new List<int>();
        foreach (var item in map)
        {
            results.Add(item.Key);
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
        
        Assert.Multiple(() =>
        {
            Assert.That(map.Count, Is.EqualTo(0));
            Assert.That(map.Contains(10), Is.False);
        });
    }

    [Test]
    public void AssignFrom_CopiesDataAndResizes()
    {
        var sourceMap = new FlatMap<int, string>();
        sourceMap.Add(10, "Ten");
        sourceMap.Add(20, "Twenty");

        var targetMap = new FlatMap<int, string>(1);
        targetMap.AssignFrom(sourceMap);

        Assert.Multiple(() =>
        {
            Assert.That(targetMap.Count, Is.EqualTo(2));
            Assert.That(targetMap[10], Is.EqualTo("Ten"));
            Assert.That(targetMap[20], Is.EqualTo("Twenty"));
        });
    }

    [Test]
    public void CopyConstructor_CopiesDataCorrectly()
    {
        var sourceMap = new FlatMap<int, string>();
        sourceMap.Add(10, "Ten");
        sourceMap.Add(20, "Twenty");

        var targetMap = new FlatMap<int, string>(sourceMap);

        Assert.Multiple(() =>
        {
            Assert.That(targetMap.Count, Is.EqualTo(2));
            Assert.That(targetMap[10], Is.EqualTo("Ten"));
            Assert.That(targetMap[20], Is.EqualTo("Twenty"));
        });
    }
}
