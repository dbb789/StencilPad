using StencilPad.Common;
using StencilPad.Spatial;
using System.ComponentModel;

namespace StencilPad.Models.Resolvers;

public class RulerResolver : SheetElementResolver
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
        : base(ruler)
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
        _settings.Changed += OnSettingsChanged;
    }

    public override void Dispose()
    {
        Detach();
        
        _ruler.GeometryChanged -= OnGeometryChanged;
        _ruler.TransformChanged -= OnTransformChanged;
        _ruler.PropertyChanged -= OnPropertyChanged;
        _settings.Changed -= OnSettingsChanged;
    }
    public override UnitBounds GetOutlineBounds(UnitTransform transform)
    {
        var bounds = base.GetOutlineBounds(transform);
        var capResource = _resourceSet.Get(GeometryResourceId.First);
        var capOffset = capResource.Size.Y + _geometryStyle.LineWidth;

        if (capResource is not null)
        {
            var min = transform.Apply(_ruler.Min);
            var max = transform.Apply(_ruler.Max);
            var direction = max - min;

            bounds = UnitBounds.Union(bounds, UnitBounds.FromCenterSize(min + direction.NormalizedTo(capOffset), capResource.Size));
            bounds = UnitBounds.Union(bounds, UnitBounds.FromCenterSize(max - direction.NormalizedTo(capOffset), capResource.Size));
        }

        return bounds;
    }

    public override void Attach(IModelWalker walker)
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

    public override void Detach()
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

        InvokeOutlineChanged();
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_ruler.Transform);

        InvokeOutlineChanged();
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

        InvokeOutlineChanged();
    }

    private void OnSettingsChanged()
    {
        _textWalker?.SetText(GetText());
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
        return UnitBounds.FromCenterSize(Unit2D.FromMillimeters(0, -6),
                                         Unit2D.FromMillimeters(100, 10));
    }

    private string GetText()
    {
        return UnitUtil.FormatSuffixScaled(_ruler.Length, _settings.UnitSettings);
    }

    private GeometrySet CreateGeometrySet()
    {
        var capResource = _resourceSet.Get(GeometryResourceId.First);
        var direction = _ruler.Max - _ruler.Min;
        var capOffset = capResource.Size.Y + _geometryStyle.LineWidth;

        _lineResolver.Line = new Line(_ruler.Min + direction.NormalizedTo(capOffset),
                                      _ruler.Max - direction.NormalizedTo(capOffset));
        
        _caps.Clear();
        _caps.Add((capResource, GetStartCapTransform()));
        _caps.Add((capResource, GetEndCapTransform()));
        
        return new GeometrySet(_lineResolver, _caps);
    }

    private UnitTransform GetStartCapTransform()
    {
        var rotation = Math.Atan2((_ruler.Max.Y - _ruler.Min.Y).Millimeters,
                                  (_ruler.Max.X - _ruler.Min.X).Millimeters) * MathUtil.Rad2Deg;
        
        var direction = _ruler.Max - _ruler.Min;
        var capPosition = _ruler.Min + direction.NormalizedTo(_geometryStyle.LineWidth);

        return new UnitTransform(capPosition, rotation - 90);
    }

    private UnitTransform GetEndCapTransform()
    {
        var rotation = Math.Atan2((_ruler.Max.Y - _ruler.Min.Y).Millimeters,
                                  (_ruler.Max.X - _ruler.Min.X).Millimeters) * MathUtil.Rad2Deg;
        
        var direction = _ruler.Max - _ruler.Min;
        var capPosition = _ruler.Max - direction.NormalizedTo(_geometryStyle.LineWidth);

        return new UnitTransform(capPosition, rotation + 90);
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
