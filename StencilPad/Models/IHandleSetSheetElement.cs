namespace StencilPad.Models;

public interface IHandleSetSheetElement : ISheetElement
{
    IHandleSet HandleSet { get; }
}
