using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Spatial;

namespace StencilPad.Export;

public class SvgExporter
{
    public class UnitMapper
    {
        public Unit2D Map(Unit2D point)
        {
            return new Unit2D(point.X, -point.Y);
        }

        public UnitBounds Map(UnitBounds bounds)
        {
            var min = Map(bounds.Min);
            var max = Map(bounds.Max);
            
            return UnitBounds.FromMinMax(min, max);
        }
    }

    private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

    private readonly SheetResolver.Factory _sheetResolverFactory;

    public SvgExporter(SheetResolver.Factory sheetResolverFactory)
    {
        _sheetResolverFactory = sheetResolverFactory;
    }
    
    public void Export(Sheet sheet, string path)
    {
        UnitBounds? sheetBounds = null;

        using var resolver = _sheetResolverFactory.Create(sheet);

        foreach (var elementResolver in resolver.Elements)
        {
            sheetBounds = UnitBounds.Union(sheetBounds, elementResolver.GetOutlineBounds());
        }

        var mapper = new UnitMapper();
        
        var bounds = sheetBounds ??
            UnitBounds.FromCenterSize(Unit2D.Zero,
                                      new Unit2D(Unit.FromMillimeters(100),
                                                 Unit.FromMillimeters(100)));

        bounds = mapper.Map(bounds);
        
        double width = bounds.Size.X.Millimeters;
        double height = bounds.Size.Y.Millimeters;
        double offsetX = bounds.Min.X.Millimeters;
        double offsetY = bounds.Min.Y.Millimeters;

        var svg = new XElement(SvgNs + "svg",
            new XAttribute("width", Mm(width)),
            new XAttribute("height", Mm(height)),
            new XAttribute("viewBox", $"{Num(offsetX)} {Num(offsetY)} {Num(width)} {Num(height)}"));
        
        using (var modelWalker = new SvgModelWalker(svg, mapper))
        {
            foreach (var elementResolver in resolver.Elements)
            {
                elementResolver.Attach(modelWalker);
                elementResolver.Detach();
            }
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), svg);

        using var stream = File.Create(path);
        
        doc.Save(stream);
    }

    private class SvgModelWalker : IModelWalker, IDisposable
    {
        private readonly XElement _svg;
        private readonly UnitMapper _mapper;
        private readonly UnitTransform _parentTransform;
        private readonly List<IDisposable> _walkers = new();
        private UnitTransform _worldTransform;

        public SvgModelWalker(XElement svg,
                              UnitMapper mapper,
                              UnitTransform parentTransform = default)
        {
            _svg = svg;
            _mapper = mapper;
            _parentTransform = parentTransform;
            _worldTransform = parentTransform;
        }

        public void SetTransform(UnitTransform localTransform)
        {
            _worldTransform = _parentTransform * localTransform;
        }

        public IModelWalker CreateModelWalker()
        {
            var walker = new SvgModelWalker(_svg, _mapper, _worldTransform);
            _walkers.Add(walker);
            return walker;
        }

        public IStyledGeometryWalker CreateStyledGeometryWalker()
        {
            var walker = new SvgGeometryWalker(_svg, _mapper, _worldTransform);
            _walkers.Add(walker);
            return walker;
        }

        public ITextWalker CreateTextWalker()
        {
            var walker = new SvgTextWalker(_svg, _mapper, _worldTransform);
            _walkers.Add(walker);
            return walker;
        }

        public IImageWalker CreateImageWalker()
        {
            var walker = new SvgImageWalker(_svg, _mapper, _worldTransform);
            _walkers.Add(walker);
            return walker;
        }

        public void Dispose()
        {
            foreach (var walker in _walkers)
            {
                walker.Dispose();
            }
            
            _walkers.Clear();
        }
    }

    private class SvgGeometryWalker : IStyledGeometryWalker, IDisposable
    {
        private readonly XElement _svg;
        private readonly UnitMapper _mapper;
        private readonly UnitTransform _transform;
        private GeometryStyle _style;

        private readonly StringBuilder _pendingPaths = new();
        private readonly List<(Shape Shape, UnitTransform Transform)> _pendingOverlays = new();
        private bool _pendingClosed;

        public SvgGeometryWalker(XElement svg, UnitMapper mapper, UnitTransform transform)
        {
            _svg = svg;
            _mapper = mapper;
            _transform = transform;
        }

        public void SetStyle(GeometryStyle style)
        {
            Flush();
            
            _style = style;
        }

        public void Create(int id, GeometrySet geometrySet)
        {
            var pathWalker = new SvgPathWalker(_mapper, _transform);

            if (geometrySet.StartPoint is not null ||
                geometrySet.EndPoint is not null)
            {
                var clampedWalker = new ClampedGeometryWalker(pathWalker);
                
                clampedWalker.SetStartEnd(geometrySet.StartPoint,
                                          geometrySet.EndPoint);
                
                geometrySet.Resolver.Walk(clampedWalker);
            }
            else
            {
                geometrySet.Resolver.Walk(pathWalker);
            }

            var pathData = pathWalker.Build();

            if (!string.IsNullOrEmpty(pathData))
            {
                if (_pendingPaths.Length > 0)
                {
                    _pendingPaths.Append(' ');
                }
                
                _pendingPaths.Append(pathData);
                _pendingClosed |= pathWalker.Closed;
            }

            foreach (var (resource, overlayTransform) in geometrySet.Overlays)
            {
                _pendingOverlays.Add((resource.Shape, _transform * overlayTransform));
            }
        }

        public void Update(int id, GeometrySet geometrySet) { }

        public void Destroy(int id) { }

        public void Dispose()
        {
            Flush();
        }

        private void Flush()
        {
            if (_pendingPaths.Length > 0)
            {
                _svg.Add(BuildPathElement(_pendingPaths.ToString(), _pendingClosed));
                _pendingPaths.Clear();
                _pendingClosed = false;
            }

            foreach (var (shape, transform) in _pendingOverlays)
            {
                AppendShape(shape, transform);
            }

            _pendingOverlays.Clear();
        }

        private void AppendShape(Shape shape, UnitTransform transform)
        {
            foreach (var polygon in shape.PolygonSet)
            {
                var pathWalker = new SvgPathWalker(_mapper, transform);
                
                polygon.Resolver.Walk(pathWalker);
                
                var pathData = pathWalker.Build();

                if (!string.IsNullOrEmpty(pathData))
                {
                    _svg.Add(BuildPathElement(pathData, pathWalker.Closed));
                }
            }
        }

        private XElement BuildPathElement(string pathData, bool closed)
        {
            var fill        = closed && _style.FillColor.A != 0 ? ColorUtil.ToHexStringOpaque(_style.FillColor) : "none";
            var stroke      = _style.LineColor.A != 0 ? ColorUtil.ToHexStringOpaque(_style.LineColor) : "none";
            var strokeWidth = Num(_style.LineWidth.Millimeters);

            var path = new XElement(SvgNs + "path",
                new XAttribute("d",              pathData),
                new XAttribute("fill",           fill),
                new XAttribute("fill-rule",      "evenodd"),
                new XAttribute("stroke",         stroke),
                new XAttribute("stroke-width",   strokeWidth),
                new XAttribute("stroke-opacity", Num(_style.LineColor.A / 255.0)));

            if (_style.LineStyle == LineStyleResourceId.Dashes)
            {
                var dash = _style.LineWidth.Millimeters;
                path.Add(new XAttribute("stroke-dasharray", $"{Num(dash * 4)} {Num(dash * 2)}"));
            }

            return path;
        }
    }

    private class SvgTextWalker : ITextWalker, IDisposable
    {
        private readonly XElement _svg;
        private readonly UnitMapper _mapper;
        private readonly UnitTransform _parentTransform;
        private UnitTransform _worldTransform;
        private TextStyle _style;
        private UnitBounds? _bounds;
        private string _text = "";

        public SvgTextWalker(XElement svg,
                             UnitMapper mapper,
                             UnitTransform parentTransform)
        {
            _svg = svg;
            _mapper = mapper;
            _parentTransform = parentTransform;
            _worldTransform = parentTransform;
            _style = new TextStyle();
        }

        public void SetTransform(UnitTransform localTransform)
        {
            _worldTransform = _parentTransform * localTransform;
        }

        public void SetStyle(TextStyle style)
        {
            _style = style;
        }

        public void SetBounds(UnitBounds? bounds)
        {
            _bounds = bounds;
        }

        public void SetText(string text)
        {
            _text = text;
        }
        
        public void Dispose()
        {
            if (string.IsNullOrEmpty(_text))
            {
                return;
            }
            
            Unit2D origin;
            
            if (_bounds is not null)
            {
                var b = _bounds.Value;
                var localX = _style.Justification switch
                {
                    Justification.Center => b.Center.X,
                    Justification.Right => b.Max.X,
                    _  => b.Min.X
                };
                origin = _worldTransform.Apply(new Unit2D(localX, b.Min.Y));
            }
            else
            {
                origin = _worldTransform.Position;
            }

            origin = _mapper.Map(origin);

            var fontSizeMm = Unit.FromFontSizePoints(_style.Size).Millimeters;

            // Measure the WPF alphabetic baseline offset from the layout box top.
            // Passing font size as mm with PixelsPerDip=1 gives all metrics directly in mm,
            // matching how WPF's DrawText(formattedText, Point(0,0)) positions text.
            var ft = new FormattedText(
                _text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(_style.Font),
                fontSizeMm,
                Brushes.Black,
                1.0);

            var x = Num(origin.X.Millimeters);
            var y = Num(origin.Y.Millimeters);
            var fontSize = Num(fontSizeMm);
            var dy = Num(ft.Baseline);

            var anchor = _style.Justification switch
            {
                Justification.Center => "middle",
                Justification.Right => "end",
                _ => "start"
            };

            var elem = new XElement(SvgNs + "text",
                new XAttribute("x", x),
                new XAttribute("y", y),
                new XAttribute("dy", dy),
                new XAttribute("font-family", _style.Font),
                new XAttribute("font-size", fontSize),
                new XAttribute("fill", ColorUtil.ToHexStringOpaque(_style.Color)),
                new XAttribute("text-anchor", anchor),
                _text);

            // SVG has no global Y-flip, so negate the angle to match WPF's flipped rendering.
            // Then normalize to (-90°, 90°] so text is always readable.
            // Strict < -90 (not <=) preserves the exact -90° case (vertical rulers).
            var svgAngle = -(double)_worldTransform.Angle;

            if (svgAngle > 90.0 || svgAngle < -90.0)
            {
                svgAngle -= Math.Sign(svgAngle) * 180.0;
            }
            
            if (Math.Abs(svgAngle) > MathUtil.Epsilon)
            {
                elem.Add(new XAttribute("transform", $"rotate({Num(svgAngle)},{x},{y})"));
            }

            _svg.Add(elem);
        }
    }

    private class SvgImageWalker : IImageWalker, IDisposable
    {
        private readonly XElement _svg;
        private readonly UnitMapper _mapper;
        private readonly UnitTransform _transform;
        private UnitBounds? _bounds;
        private byte[]? _imageData;
        private double _opacity = 1.0;

        public SvgImageWalker(XElement svg,
                              UnitMapper mapper,
                              UnitTransform transform)
        {
            _svg = svg;
            _mapper = mapper;
            _transform = transform;
        }

        public void SetBounds(UnitBounds? bounds)
        {
            _bounds = bounds;
        }

        public void SetImageData(byte[] imageData)
        {
            _imageData = imageData;
        }

        public void SetOpacity(double opacity)
        {
            _opacity = opacity;
        }
        
        public void Dispose()
        {
            if (_bounds is null || _imageData is null || _imageData.Length == 0)
            {
                return;
            }

            var b = _mapper.Map(_bounds.Value);
            var mime = ImageUtil.GetMimeType(_imageData);
            var base64 = Convert.ToBase64String(_imageData);

            var x = Num(b.Min.X.Millimeters);
            var y = Num(b.Min.Y.Millimeters);
            var width = Num(b.Size.X.Millimeters);
            var height = Num(b.Size.Y.Millimeters);

            var elem = new XElement(SvgNs + "image",
                new XAttribute("x", x),
                new XAttribute("y", y),
                new XAttribute("width", width),
                new XAttribute("height", height),
                new XAttribute("href", $"data:{mime};base64,{base64}"));

            if (Math.Abs(_opacity - 1.0) > 1e-6)
            {
                elem.Add(new XAttribute("opacity", Num(_opacity)));
            }

            var pos = _mapper.Map(_transform.Position);
            var tx = Num(pos.X.Millimeters);
            var ty = Num(pos.Y.Millimeters);

            if (_transform.Angle != 0)
            {
                elem.Add(new XAttribute("transform",
                                        $"translate({tx},{ty}) rotate({Num((double)_transform.Angle)})"));
            }
            else if (!pos.ApproximatelyEquals(Unit2D.Zero))
            {
                elem.Add(new XAttribute("transform", $"translate({tx},{ty})"));
            }
            
            _svg.Add(elem);
        }
    }

    private class SvgPathWalker : IGeometryWalker
    {
        private readonly UnitTransform _transform;
        private readonly UnitMapper _mapper;
        private readonly StringBuilder _sb = new();
        private bool _started;
        private bool _closed;

        public bool Closed => _closed;

        public SvgPathWalker(UnitMapper mapper,
                             UnitTransform transform)
        {
            _mapper = mapper;
            _transform = transform;
        }

        public bool Begin(int segmentCount, bool closed)
        {
            _started = false;
            _closed = closed;
            return segmentCount > 0;
        }

        public bool Segment(int segmentIndex, PolygonSegment segment)
        {
            if (segment.IsLine)
            {
                var line = segment.Line;

                if (!_started)
                {
                    var from = _mapper.Map(_transform.Apply(line.Start));
                    _sb.Append($"M {Coord(from)}");
                    _started = true;
                }

                var to = _mapper.Map(_transform.Apply(line.End));
                _sb.Append($" L {Coord(to)}");
            }
            else if (segment.IsBezier)
            {
                var b = segment.Bezier;

                if (!_started)
                {
                    var from = _mapper.Map(_transform.Apply(b.P0));
                    _sb.Append($"M {Coord(from)}");
                    _started = true;
                }

                var p1 = _mapper.Map(_transform.Apply(b.P1));
                var p2 = _mapper.Map(_transform.Apply(b.P2));
                var p3 = _mapper.Map(_transform.Apply(b.P3));

                _sb.Append($" C {Coord(p1)} {Coord(p2)} {Coord(p3)}");
            }
            else if (segment.IsArc)
            {
                AppendArc(segment.Arc);
            }

            return true;
        }

        public string Build()
        {
            if (!_started)
            {
                return string.Empty;
            }

            if (_closed)
            {
                _sb.Append(" Z");
            }

            return _sb.ToString();
        }

        private void AppendArc(Arc arc)
        {
            if (!_started)
            {
                var from = _mapper.Map(_transform.Apply(arc.Start));
                
                _sb.Append($"M {Coord(from)}");
                _started = true;
            }

            var end = _mapper.Map(_transform.Apply(arc.End));
            var r = arc.Radius.Millimeters;
            var sweep  = MathUtil.SignedAngleDifference(arc.EndAngle, arc.StartAngle) > 0 ? 1 : 0;

            _sb.Append($" A {Num(r)},{Num(r)} 0 0 {sweep} {Coord(end)}");
        }
    }

    private static string Coord(Unit2D point) => $"{Num(point.X.Millimeters)},{Num(point.Y.Millimeters)}";
    
    private static string Mm(double value) => $"{Num(value)}mm";

    private static string Num(double value) => value.ToString("G6", CultureInfo.InvariantCulture);
}
