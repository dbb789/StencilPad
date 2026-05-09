namespace StencilPad.Models.Operations;

// This operation is a recorded state change and therefore doesn't need to be
// executed.
public interface IMementoOperation : IOperation
{ }
