namespace StencilPad.Services;

public class HintService : IHintService
{
    private string _hint = "";
    
    public event Action<string>? HintChanged;
    
    public void SetHint(string text)
    {
        if (_hint == text)
        {
            return;
        }
        
        _hint = text;
        HintChanged?.Invoke(_hint);
    }
    
    public void ClearHint()
    {
        SetHint("");
    }

    public void ClearAll()
    {
        ClearHint();
    }
}
