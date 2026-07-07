namespace StencilPad.Models.Operations;

public interface IOperation
{
    void Execute(Project project, out Sheet? targetSheet);
    IOperation Invert();
}
