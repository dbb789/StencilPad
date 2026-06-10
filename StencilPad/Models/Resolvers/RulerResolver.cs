using System.ComponentModel;
using StencilPad.Common;
using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public class RulerResolver : IModelResolver
{
    private const int GeometryId = 1;

    private readonly Ruler _ruler;
    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private readonly LineResolver _lineResolver;
    private readonly List<(GeometryResource, UnitTransform)> _caps;

    private IModelWalker? _walker;
    private IStyledGeometryWalker? _geometryWalker;
    private ITextWalker? _textWalker;
    
    private GeometryStyle _geometryStyle;
    private TextStyle _textStyle;
    
    public RulerResolver(Ruler ruler,
                         ISettings settings,
                         IResourceSet resourceSet)
    {
        _ruler = ruler;
        _settings = settings;
        _resourceSet = resourceSet;
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
        _textWalker.SetBounds(GetTextBounds());
        _textWalker.SetStyle(_textStyle);
        _textWalker.SetText(GetText());
    }

    public void Detach()
    {
        _geometryWalker?.Destroy(GeometryId);
        _geometryWalker = null;
        _textWalker = null;
        _walker = null;
    }
    
    private void OnGeometryChanged(ISheetElement element)
    {
        _geometryWalker?.Update(GeometryId, CreateGeometrySet());
        _textWalker?.SetTransform(GetTextTransform());
        _textWalker?.SetBounds(GetTextBounds());
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

    private UnitBounds GetTextBounds()
    {
        return UnitBounds.FromCenterSize(Unit2D.FromMillimeters(0, 51),
                                         Unit2D.FromMillimeters(1000, 100));
    }

    private string GetText()
    {
        return $"{_ruler.Length.Millimeters:F1} mm";
    }

    private GeometrySet CreateGeometrySet()
    {
        var capResource = _resourceSet.Get(GeometryResourceId.First);
        var direction = _ruler.Max - _ruler.Min;
        
        _lineResolver.Line = new Line(_ruler.Min + direction.NormalizedTo(capResource.Size.Y),
                                      _ruler.Max - direction.NormalizedTo(capResource.Size.Y));
        
        _caps.Clear();
        _caps.Add((capResource, GetStartCapTransform()));
        _caps.Add((capResource, GetEndCapTransform()));
        
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
            Justification = Justification.Center,
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
