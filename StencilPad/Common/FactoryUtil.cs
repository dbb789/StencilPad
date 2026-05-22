using Microsoft.Extensions.DependencyInjection;

namespace StencilPad.Common;

public static class FactoryUtil
{
    public static void AddFactory<TService>(IServiceCollection services) where TService : class
    {
        services.AddTransient<TService>();
        services.AddSingleton<Factory<TService>>(sp => new(() => sp.GetRequiredService<TService>()));
    }
}
