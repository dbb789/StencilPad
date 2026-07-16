namespace StencilPad.Tests.Collections;

using StencilPad.Collections;
using NUnit.Framework;

[TestFixture]
public class ObjectPoolTests
{
    [Test]
    public void TryGet_EmptyPool_ReturnsNull()
    {
        var pool = new ObjectPool<object>();
        
        var obj = pool.TryGet();
        
        Assert.That(obj, Is.Null);
    }

    [Test]
    public void Recycle_And_TryGet_ReturnsSameInstance()
    {
        var pool = new ObjectPool<object>();
        var obj = new object();
        
        pool.Recycle(obj);
        var retrievedObj = pool.TryGet();
        
        Assert.Multiple(() =>
        {
            Assert.That(retrievedObj, Is.Not.Null);
            Assert.That(retrievedObj, Is.SameAs(obj));
        });
    }

    [Test]
    public void TryGet_AfterMultipleRecycles_ReturnsInstancesInLifoOrder()
    {
        var pool = new ObjectPool<object>();
        var obj1 = new object();
        var obj2 = new object();
        
        pool.Recycle(obj1);
        pool.Recycle(obj2);
        
        var retrieved1 = pool.TryGet();
        var retrieved2 = pool.TryGet();
        var retrieved3 = pool.TryGet();
        
        Assert.Multiple(() =>
        {
            Assert.That(retrieved1, Is.SameAs(obj2));
            Assert.That(retrieved2, Is.SameAs(obj1));
            Assert.That(retrieved3, Is.Null);
        });
    }
}
