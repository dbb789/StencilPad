namespace StencilPad.Models.Operations;

public interface IOperation
{
    void Execute(Project project);
    IOperation Invert();
}
