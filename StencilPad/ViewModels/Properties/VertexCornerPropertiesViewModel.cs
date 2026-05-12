using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public record CornerTypeItem(CornerType Value, string Description);

public class VertexCornerPropertiesViewModel : ViewModelBase
{
    private readonly Sheet _sheet;
    private readonly IEnumerable<VertexCornerTarget> _targets;
    private readonly IOperationService _operationService;
    private CornerType _cornerType;
    private CornerSize _cornerSize;

    public static IReadOnlyList<CornerTypeItem> CornerTypes { get; } =
    [
        new(CornerType.None, "None"),
        new(CornerType.Rounded, "Rounded"),
        new(CornerType.Beveled, "Beveled"),
    ];

    public string Title => _targets.Count() == 1
        ? "Vertex Corner"
        : $"Vertex Corner ({_targets.Count()} selected)";

    public CornerType CornerType
    {
        get => _cornerType;
        set
        {
            SetProperty(ref _cornerType, value);

            var context = new EditSheetElementContext(_sheet, _targets.Select(t => t.Element));
            
            foreach (var target in _targets)
            {
                var polygon = target.Polygon;
                var vertex = polygon.Vertices[target.VertexIndex];

                polygon.Vertices[target.VertexIndex] = vertex with { CornerType = value };
            }

            _operationService.Push(context.FlushOperation());
        }
    }

    public CornerSize CornerSize
    {
        get => _cornerSize;
        set
        {
            SetProperty(ref _cornerSize, value);

            var context = new EditSheetElementContext(_sheet, _targets.Select(t => t.Element));

            foreach (var target in _targets)
            {
                var polygon = target.Polygon;
                var vertex = polygon.Vertices[target.VertexIndex];

                polygon.Vertices[target.VertexIndex] = vertex with { CornerSize = value };
            }

            _operationService.Push(context.FlushOperation());
        }
    }

    public VertexCornerPropertiesViewModel(Sheet sheet,
                                           IEnumerable<VertexCornerTarget> targets,
                                           IOperationService operationService)
    {
        var first = targets.First();
        var vertex = first.Polygon.Vertices[first.VertexIndex];
        
        _cornerType = vertex.CornerType;
        _cornerSize = vertex.CornerSize;
        
        _sheet = sheet;
        _targets = targets;
        _operationService = operationService;
    }
}
