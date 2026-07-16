namespace StencilPad.Tests.Common;

using Microsoft.Extensions.DependencyInjection;
using StencilPad.Common;

public class FactoryTests
{
    private class DummyService { }

    [Test]
    public void Factory_Create_InvokesDelegate()
    {
        int count = 0;
        var factory = new Factory<DummyService>(() => 
        {
            count++;
            return new DummyService();
        });

        var instance1 = factory.Create();
        var instance2 = factory.Create();

        Assert.Multiple(() =>
        {
            Assert.That(instance1, Is.Not.Null);
            Assert.That(instance2, Is.Not.Null);
            Assert.That(instance1, Is.Not.SameAs(instance2));
            Assert.That(count, Is.EqualTo(2));
        });
    }

    [Test]
    public void FactoryUtil_AddFactory_RegistersTransientServiceAndSingletonFactory()
    {
        var services = new ServiceCollection();
        FactoryUtil.AddFactory<DummyService>(services);

        using var provider = services.BuildServiceProvider();
        
        var factory1 = provider.GetRequiredService<Factory<DummyService>>();
        var factory2 = provider.GetRequiredService<Factory<DummyService>>();

        Assert.That(factory1, Is.SameAs(factory2), "Factory should be registered as Singleton");

        var instance1 = factory1.Create();
        var instance2 = factory1.Create();

        Assert.That(instance1, Is.Not.SameAs(instance2), "Service should be registered as Transient");
    }
}
