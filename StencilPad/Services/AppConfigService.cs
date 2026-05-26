using StencilPad.Common;

namespace StencilPad.Services;

public class AppConfigService : IAppConfigService
{
    public IAppConfig Config { get; private set; } = new AppConfig();

    public event Action? ConfigChanged;

    public void ApplyConfig(IAppConfig newConfig)
    {
        Config = newConfig;
        ConfigChanged?.Invoke();
    }
}
