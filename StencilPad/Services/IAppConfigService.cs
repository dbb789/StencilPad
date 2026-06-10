using StencilPad.Common;

namespace StencilPad.Services;

public interface IAppConfigService
{
    AppConfig Config { get; }

    event Action? Applied;
}
