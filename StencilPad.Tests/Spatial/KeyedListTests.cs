namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;

public class KeyedListTests
{
    [Test]
    public void Add_IncreasesCountAndAssignsKeys()
    {
        var list = new KeyedList<string>();
        list.Add("A");
        list.Add("B");

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo("A"));
            Assert.That(list[1], Is.EqualTo("B"));
            Assert.That(list.KeyAt(0), Is.Not.EqualTo(list.KeyAt(1)));
        });
    }

    [Test]
    public void IndexOfKey_ReturnsCorrectIndex()
    {
        var list = new KeyedList<string>();
        list.Add("A");
        list.Add("B");
        var keyA = list.KeyAt(0);
        var keyB = list.KeyAt(1);

        Assert.Multiple(() =>
        {
            Assert.That(list.IndexOfKey(keyA), Is.EqualTo(0));
            Assert.That(list.IndexOfKey(keyB), Is.EqualTo(1));
        });
    }

    [Test]
    public void Insert_UpdatesIndicesCorrectly()
    {
        var list = new KeyedList<string>();
        list.Add("A");
        list.Add("C");
        
        var keyC = list.KeyAt(1);
        
        list.Insert(1, "B");
        var keyB = list.KeyAt(1);

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0], Is.EqualTo("A"));
            Assert.That(list[1], Is.EqualTo("B"));
            Assert.That(list[2], Is.EqualTo("C"));
            
            Assert.That(list.IndexOfKey(keyB), Is.EqualTo(1), "Key B should be at index 1");
            Assert.That(list.IndexOfKey(keyC), Is.EqualTo(2), "Key C should be at index 2");
        });
    }

    [Test]
    public void RemoveAt_UpdatesIndicesCorrectly()
    {
        var list = new KeyedList<string>();
        list.Add("A");
        list.Add("B");
        list.Add("C");
        
        var keyC = list.KeyAt(2);
        
        list.RemoveAt(1); // Remove B

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo("A"));
            Assert.That(list[1], Is.EqualTo("C"));
            Assert.That(list.IndexOfKey(keyC), Is.EqualTo(1), "Key C should be at index 1");
        });
    }

    [Test]
    public void At_HandlesNegativeAndOverflowIndices()
    {
        var list = new KeyedList<string>();
        list.Add("A");
        list.Add("B");
        list.Add("C");

        Assert.Multiple(() =>
        {
            Assert.That(list.At(0), Is.EqualTo("A"));
            Assert.That(list.At(3), Is.EqualTo("A"));
            Assert.That(list.At(-1), Is.EqualTo("C"));
            Assert.That(list.At(-4), Is.EqualTo("C"));
        });
    }

    [Test]
    public void IndexerSetter_TriggersItemReassigned()
    {
        var list = new KeyedList<string>();
        list.Add("A");
        var key = list.KeyAt(0);
        
        int calledIndex = -1;
        ulong calledKey = 0;
        string? oldVal = null;
        string? newVal = null;

        list.ItemReassigned += (idx, k, oldV, newV) =>
        {
            calledIndex = idx;
            calledKey = k;
            oldVal = oldV;
            newVal = newV;
        };

        list[0] = "A_Updated";

        Assert.Multiple(() =>
        {
            Assert.That(list[0], Is.EqualTo("A_Updated"));
            Assert.That(calledIndex, Is.EqualTo(0));
            Assert.That(calledKey, Is.EqualTo(key));
            Assert.That(oldVal, Is.EqualTo("A"));
            Assert.That(newVal, Is.EqualTo("A_Updated"));
        });
    }

    [Test]
    public void DeepClone_CreatesIndependentCopy()
    {
        var list = new KeyedList<string>();
        list.Add("A");
        
        var clone = list.DeepClone();
        clone.Add("B");

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(clone.Count, Is.EqualTo(2));
            Assert.That(clone[0], Is.EqualTo("A"));
            Assert.That(clone[1], Is.EqualTo("B"));
        });
    }

    [Test]
    public void At_OnEmptyList_ThrowsException()
    {
        var list = new KeyedList<string>();
        Assert.Throws<DivideByZeroException>(() => list.At(0));
    }

    [Test]
    public void AssignFrom_CopiesDataAndCounter()
    {
        var source = new KeyedList<string>();
        source.Add("A");
        source.Add("B");
        var keyB = source.KeyAt(1);

        var target = new KeyedList<string>();
        target.AssignFrom(source);

        Assert.Multiple(() =>
        {
            Assert.That(target.Count, Is.EqualTo(2));
            Assert.That(target[1], Is.EqualTo("B"));
            Assert.That(target.IndexOfKey(keyB), Is.EqualTo(1));
            
            // Verify counter is copied by adding to target
            target.Add("C");
            Assert.That(target.KeyAt(2), Is.EqualTo(keyB + 1));
        });
    }

    [Test]
    public void InsertAndRemove_ComplexSequence_MaintainsConsistency()
    {
        var list = new KeyedList<int>();
        for (int i = 0; i < 5; i++) list.Add(i);
        
        // [0, 1, 2, 3, 4]
        var key2 = list.KeyAt(2);
        var key4 = list.KeyAt(4);
        
        list.RemoveAt(0); // [1, 2, 3, 4]
        list.Insert(0, 10); // [10, 1, 2, 3, 4]
        list.RemoveAt(2); // [10, 1, 3, 4]
        
        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(4));
            Assert.That(list.IndexOfKey(key4), Is.EqualTo(3));
            Assert.Throws<KeyNotFoundException>(() => list.IndexOfKey(key2));
            Assert.That(list[2], Is.EqualTo(3));
        });
    }
}
