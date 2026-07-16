namespace StencilPad.Tests.Collections;

using StencilPad.Collections;
using NUnit.Framework;

[TestFixture]
public class ReadOnlyFlatSetTests
{
    private class TestReadOnlyFlatSet<T> : ReadOnlyFlatSet<T>
    {
        public TestReadOnlyFlatSet(int capacity) : base(capacity)
        {
        }

        public TestReadOnlyFlatSet(ReadOnlyFlatSet<T> other) : base(other)
        {
        }

        // Exposing a method to populate data for testing
        public void AddData(T element)
        {
            if (_dataLength >= _data.Length)
            {
                Array.Resize(ref _data, Math.Max(4, _data.Length * 2));
            }
            _data[_dataLength++] = element;
            Array.Sort(_data, 0, _dataLength, _comparer);
        }
    }

    [Test]
    public void Indexer_ReturnsCorrectElement()
    {
        var set = new TestReadOnlyFlatSet<int>(2);
        set.AddData(10);
        set.AddData(20);

        Assert.Multiple(() =>
        {
            Assert.That(set[0], Is.EqualTo(10));
            Assert.That(set[1], Is.EqualTo(20));
        });
    }

    [Test]
    public void Count_ReturnsCorrectLength()
    {
        var set = new TestReadOnlyFlatSet<int>(4);
        set.AddData(10);
        set.AddData(20);
        set.AddData(30);

        Assert.That(set.Count, Is.EqualTo(3));
    }

    [Test]
    public void Contains_ReturnsTrueForExistingElements()
    {
        var set = new TestReadOnlyFlatSet<int>(4);
        set.AddData(10);
        set.AddData(20);

        Assert.Multiple(() =>
        {
            Assert.That(set.Contains(10), Is.True);
            Assert.That(set.Contains(20), Is.True);
            Assert.That(set.Contains(30), Is.False);
        });
    }

    [Test]
    public void CopyConstructor_CreatesExactCopy()
    {
        var original = new TestReadOnlyFlatSet<int>(4);
        original.AddData(10);
        original.AddData(20);

        var copy = new TestReadOnlyFlatSet<int>(original);

        Assert.Multiple(() =>
        {
            Assert.That(copy.Count, Is.EqualTo(2));
            Assert.That(copy[0], Is.EqualTo(10));
            Assert.That(copy[1], Is.EqualTo(20));
        });
    }

    [Test]
    public void GetEnumerator_YieldsAllElements()
    {
        var set = new TestReadOnlyFlatSet<int>(4);
        set.AddData(10);
        set.AddData(20);

        var list = new List<int>();
        foreach (var item in set)
        {
            list.Add(item);
        }

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(10));
            Assert.That(list[1], Is.EqualTo(20));
        });
    }
}
