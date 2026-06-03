using System.ComponentModel;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Models.Resolvers;

public class RulerResolver : IModelResolver
{
    private const int GeometryId = 1;

    private readonly Ruler _ruler;
    private readonly IResourceService _resourceService;
    private readonly LineResolver _lineResolver;
    private readonly List<(GeometryResource, UnitTransform)> _caps;

    private IModelWalker? _walker;
    private IStyledGeometryWalker? _geometryWalker;
    private ITextWalker? _textWalker;
    
    private GeometryStyle _geometryStyle;
    private TextStyle _textStyle;
    
    public RulerResolver(Ruler ruler, IResourceService resourceService)
    {
        _ruler = ruler;
        _resourceService = resourceService;
        _lineResolver = new();
        _caps = new();
        _geometryStyle = CreateGeometryStyle();
        _textStyle = CreateTextStyle();
            
        _ruler.GeometryChanged += OnGeometryChanged;
        _ruler.TransformChanged += OnTransformChanged;
        _ruler.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        Detach();
        
        _ruler.GeometryChanged -= OnGeometryChanged;
        _ruler.TransformChanged -= OnTransformChanged;
        _ruler.PropertyChanged -= OnPropertyChanged;
    }

    public void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_ruler.Transform);
        
        _geometryWalker = walker.CreateStyledGeometryWalker();
        _geometryWalker.SetStyle(_geometryStyle);
        _geometryWalker.Create(GeometryId, CreateGeometrySet());

        _textWalker = walker.CreateTextWalker();
        _textWalker.SetTransform(GetTextTransform());
        _textWalker.SetStyle(_textStyle);
        _textWalker.SetText(GetText());
    }

    public void Detach()
    {
        _geometryWalker?.Destroy(GeometryId);
        _geometryWalker = null;
        _walker = null;
    }
    
    private void OnGeometryChanged(ISheetElement element)
    {
        _geometryWalker?.Update(GeometryId, CreateGeometrySet());
        _textWalker?.SetTransform(GetTextTransform());
        _textWalker?.SetText(GetText());
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_ruler.Transform);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsStyleProperty(e.PropertyName))
        {
            _geometryStyle = CreateGeometryStyle();
            _geometryWalker?.SetStyle(_geometryStyle);

            _textStyle = CreateTextStyle();
            _textWalker?.SetStyle(_textStyle);
        }
        else
        {
            _geometryWalker?.Update(GeometryId, CreateGeometrySet());
        }
    }

    private UnitTransform GetTextTransform()
    {
        var mid = (_ruler.Min + _ruler.Max) / 2;
        var rotation = Math.Atan2((_ruler.Max.Y - _ruler.Min.Y).Millimeters,
                                  (_ruler.Max.X - _ruler.Min.X).Millimeters) * MathUtil.Rad2Deg;
        
        return new UnitTransform(mid, rotation);
    }

    private string GetText()
    {
        return $"{_ruler.Length.Millimeters:F1} mm";
    }

    private GeometrySet CreateGeometrySet()
    {
        _lineResolver.Line = new Line(_ruler.Min, _ruler.Max);

        _caps.Clear();
        _caps.Add((_resourceService.Get(GeometryResourceId.First), GetStartCapTransform()));
        _caps.Add((_resourceService.Get(GeometryResourceId.First), GetEndCapTransform()));
        
        return new GeometrySet(_lineResolver, _caps);
    }

    private UnitTransform GetStartCapTransform()
    {
        var rotation = Math.Atan2((_ruler.Max.Y - _ruler.Min.Y).Millimeters,
                                  (_ruler.Max.X - _ruler.Min.X).Millimeters) * MathUtil.Rad2Deg;

        return new UnitTransform(_ruler.Min, rotation - 90);
    }

    private UnitTransform GetEndCapTransform()
    {
        var rotation = Math.Atan2((_ruler.Max.Y - _ruler.Min.Y).Millimeters,
                                  (_ruler.Max.X - _ruler.Min.X).Millimeters) * MathUtil.Rad2Deg;

        return new UnitTransform(_ruler.Max, rotation + 90);
    }

    private GeometryStyle CreateGeometryStyle()
    {
        return new GeometryStyle
        {
            LineColor = _ruler.Color
        };
    }
    
    private TextStyle CreateTextStyle()
    {
        return new TextStyle
        {
            Font = _ruler.FontName,
            Size = _ruler.FontSize,
            Color = _ruler.Color
        };
    }
    
    private static bool IsStyleProperty(string? propertyName)
    {
        return propertyName == nameof(Ruler.FontName) ||
            propertyName == nameof(Ruler.FontSize) ||
            propertyName == nameof(Ruler.Color);
    }
}
