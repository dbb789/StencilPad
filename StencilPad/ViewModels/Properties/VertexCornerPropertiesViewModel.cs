using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public record CornerTypeItem(CornerType Value, string Description);

public class VertexCornerPropertiesViewModel : ViewModelBase
{
    public static IReadOnlyList<CornerTypeItem> CornerTypes { get; } =
    [
        new(CornerType.None, "None"),
        new(CornerType.Rounded, "Rounded"),
        new(CornerType.Beveled, "Beveled"),
    ];

    public string Title => "Corner Properties";

    private CornerType _cornerType;
    public CornerType CornerType
    {
        get => _cornerType;
        set
        {
            SetProperty(ref _cornerType, value);

            var elements = _sheet.Selection.OfType<IPolygonSheetElement>();
            
            using var context = _operationService.CreateEditContext(_sheet, elements);
            
            foreach (var element in elements)
            {
                foreach (var polygon in element.PolygonSet)
                {
                    foreach (var vertexIndex in polygon.GetSelectedVertices())
                    {
                        var vertex = polygon.Vertices[vertexIndex];

                        polygon.Vertices[vertexIndex] = vertex with { CornerType = value };
                    }
                }
            }
        }
    }

    private CornerSize _cornerSize;
    public CornerSize CornerSize
    {
        get => _cornerSize;
        set
        {
            SetProperty(ref _cornerSize, value);
            
            var elements = _sheet.Selection.OfType<IPolygonSheetElement>();
            
            using var context = _operationService.CreateEditContext(_sheet, elements);

            foreach (var element in elements)
            {
                foreach (var polygon in element.PolygonSet)
                {
                    foreach (var vertexIndex in polygon.GetSelectedVertices())
                    {
                        var vertex = polygon.Vertices[vertexIndex];

                        polygon.Vertices[vertexIndex] = vertex with { CornerSize = value };
                    }
                }
            }
        }
    }

    private readonly Sheet _sheet;
    private readonly IOperationService _operationService;

    public VertexCornerPropertiesViewModel(Sheet sheet,
                                           IOperationService operationService)
    {
        _sheet = sheet;
        _operationService = operationService;

        var cornerTypes = new List<CornerType>();
        var cornerSizes = new List<CornerSize>();
        
        var elements = _sheet.Selection.OfType<IPolygonSheetElement>();

        foreach (var element in elements)
        {
            foreach (var polygon in element.PolygonSet)
            {
                foreach (var vertexIndex in polygon.GetSelectedVertices())
                {
                    var vertex = polygon.Vertices[vertexIndex];

                    cornerTypes.Add(vertex.CornerType);
                    cornerSizes.Add(vertex.CornerSize);
                }
            }
        }

        _cornerType = Mode(cornerTypes, CornerType.None);
        _cornerSize = Mode(cornerSizes, CornerSize.Zero);
    }
    
    private T Mode<T>(IEnumerable<T> values, T defaultValue) where T : notnull
    {
        var map = new Dictionary<T, int>();

        foreach (var value in values)
        {
            if (map.TryGetValue(value, out var count))
            {
                map[value] = count + 1;
            }
            else
            {
                map[value] = 1;
            }
        }

        T highest = defaultValue;
        int highestCount = 0;

        foreach (var (value, count) in map)
        {
            if (count > highestCount)
            {
                highest = value;
                highestCount = count;
            }
        }

        return highest;
    }
}
