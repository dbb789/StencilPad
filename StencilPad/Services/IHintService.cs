namespace StencilPad.Services;

public interface IHintService
{
    void SetHint(string text);
    void ClearHint();
    void ClearAll();
}
