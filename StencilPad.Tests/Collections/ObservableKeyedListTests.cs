namespace StencilPad.Tests.Collections;

using StencilPad.Collections;
using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class ObservableKeyedListTests
{
    [Test]
    public void Add_IncreasesCountAndTriggersListChanged()
    {
        var list = new ObservableKeyedList<string, string>();
        
        bool eventFired = false;
        list.ListChanged += args =>
        {
            eventFired = true;
            Assert.Multiple(() =>
            {
                Assert.That(args.Action, Is.EqualTo(ObservableListChangedAction.Add));
                Assert.That(args.Item, Is.EqualTo("Value1"));
                Assert.That(args.NewIndex, Is.EqualTo(0));
            });
        };

        list.Add("Key1", "Value1");

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo("Value1"));
            Assert.That(eventFired, Is.True);
        });
    }

    [Test]
    public void Remove_ExistingKey_RemovesElementAndTriggersEvents()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");

        bool removingFired = false;
        bool changedFired = false;

        list.ElementRemoving += item =>
        {
            removingFired = true;
            Assert.That(item, Is.EqualTo("Value1"));
        };

        list.ListChanged += args =>
        {
            changedFired = true;
            Assert.Multiple(() =>
            {
                Assert.That(args.Action, Is.EqualTo(ObservableListChangedAction.Remove));
                Assert.That(args.Item, Is.EqualTo("Value1"));
                Assert.That(args.OldIndex, Is.EqualTo(-1));
                Assert.That(args.NewIndex, Is.EqualTo(-1));
            });
        };

        bool removed = list.Remove("Key1");

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.True);
            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(removingFired, Is.True);
            Assert.That(changedFired, Is.True);
        });
    }

    [Test]
    public void Remove_NonExistingKey_ReturnsFalse()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");

        bool removed = list.Remove("Key2");

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.False);
            Assert.That(list.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void Insert_InsertsAtCorrectIndexAndTriggersEvent()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");
        list.Add("Key3", "Value3");

        bool eventFired = false;
        list.ListChanged += args =>
        {
            eventFired = true;
            Assert.Multiple(() =>
            {
                Assert.That(args.Action, Is.EqualTo(ObservableListChangedAction.Add));
                Assert.That(args.Item, Is.EqualTo("Value2"));
                Assert.That(args.NewIndex, Is.EqualTo(1));
            });
        };

        list.Insert(1, "Key2", "Value2");

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[1], Is.EqualTo("Value2"));
            Assert.That(eventFired, Is.True);
        });
    }

    [Test]
    public void Move_UpdatesOrderAndTriggersEvent()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");
        list.Add("Key2", "Value2");
        list.Add("Key3", "Value3");

        bool eventFired = false;
        list.ListChanged += args =>
        {
            eventFired = true;
            Assert.Multiple(() =>
            {
                Assert.That(args.Action, Is.EqualTo(ObservableListChangedAction.Move));
                Assert.That(args.Item, Is.EqualTo("Value1"));
                Assert.That(args.OldIndex, Is.EqualTo(0));
                Assert.That(args.NewIndex, Is.EqualTo(2));
            });
        };

        list.Move(0, 2);

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0], Is.EqualTo("Value2"));
            Assert.That(list[1], Is.EqualTo("Value3"));
            Assert.That(list[2], Is.EqualTo("Value1"));
            Assert.That(eventFired, Is.True);
        });
    }

    [Test]
    public void TryGetValue_ReturnsCorrectValue()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");

        Assert.Multiple(() =>
        {
            Assert.That(list.TryGetValue("Key1", out var val1), Is.True);
            Assert.That(val1, Is.EqualTo("Value1"));

            Assert.That(list.TryGetValue("Key2", out var val2), Is.False);
            Assert.That(val2, Is.Null);
        });
    }

    [Test]
    public void Clear_RemovesAllElements()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");
        list.Add("Key2", "Value2");

        int removeCount = 0;
        list.ListChanged += args =>
        {
            if (args.Action == ObservableListChangedAction.Remove)
            {
                removeCount++;
            }
        };

        list.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(removeCount, Is.EqualTo(2));
        });
    }

    [Test]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");
        list.Add("Key2", "Value2");

        Assert.Multiple(() =>
        {
            Assert.That(list.IndexOf("Value2"), Is.EqualTo(1));
            Assert.That(list.IndexOf("Value3"), Is.EqualTo(-1));
        });
    }

    [Test]
    public void GetEnumerator_EnumeratesAllValues()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");
        list.Add("Key2", "Value2");

        var results = new List<string>();
        foreach (var value in list)
        {
            results.Add(value);
        }

        Assert.That(results, Is.EqualTo(new[] { "Value1", "Value2" }));
    }

    [Test]
    public void Enumerator_ThrowsIfModifiedDuringEnumeration()
    {
        var list = new ObservableKeyedList<string, string>();
        list.Add("Key1", "Value1");
        list.Add("Key2", "Value2");

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var value in list)
            {
                if (value == "Value1")
                {
                    list.Add("Key3", "Value3");
                }
            }
        });
    }
}
