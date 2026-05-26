using StencilPad.Common;

namespace StencilPad.Services;

public interface IAppConfigService
{
    IAppConfig Config { get; }

    event Action? ConfigChanged;
}
