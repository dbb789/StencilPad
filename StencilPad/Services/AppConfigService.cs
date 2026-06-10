namespace StencilPad.Services;

public class AppConfigService : IAppConfigService
{
    public AppConfig Config { get; private set; } = new();

    public event Action? Applied;

    public void Apply()
    {
        Applied?.Invoke();
    }
}
