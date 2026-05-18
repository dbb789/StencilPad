namespace StencilPad.Models;

public interface IHandleKey
{
    HandleKeyType KeyType { get; }
    
    ulong Pack();
    void Unpack(ulong key);
}
