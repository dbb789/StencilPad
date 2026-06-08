using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Canvases.Common;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Canvases.UI;

public class CanvasGrid : ContentControl, IUnitSnap
{
    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }
    
    private readonly IAppConfigService _appConfigService;
    private readonly IViewport _viewport;

    private Pen _pageOutlinePen = null!;
    private Pen _minorPen = null!;
    private Pen _majorPen = null!;
    private Pen _axisPen = null!;
    
    public CanvasGrid(IAppConfigService appConfigService,
                      IViewport viewport)
    {
        _appConfigService = appConfigService;
        _viewport = viewport;

        _pageOutlinePen = new Pen(Brushes.LightGray, 1) { DashStyle = DashStyles.Solid };
        _pageOutlinePen.Freeze();
        
        BuildPens();
        
        Loaded += (s, e) =>
        {
            _appConfigService.ConfigChanged += OnConfigChanged;
        };

        Unloaded += (s, e) =>
        {
            _appConfigService.ConfigChanged -= OnConfigChanged;
        };
    }

    private void BuildPens()
    {
        var gridLineColor = _appConfigService.Config.GridLineColor;
        
        var minorBrush = new SolidColorBrush(ColorUtil.WithAlpha(gridLineColor, 64));
        minorBrush.Freeze();

        var majorBrush = new SolidColorBrush(ColorUtil.WithAlpha(gridLineColor, 128));
        majorBrush.Freeze();

        var axisBrush = new SolidColorBrush(ColorUtil.WithAlpha(gridLineColor, 192));
        axisBrush.Freeze();
        
        _minorPen = new Pen(minorBrush, 0.5) { DashStyle = DashStyles.Solid };
        _minorPen.Freeze();
        
        _majorPen = new Pen(majorBrush, 0.5) { DashStyle = DashStyles.Solid };
        _majorPen.Freeze();
        
        _axisPen  = new Pen(axisBrush, 1) { DashStyle = DashStyles.Solid };
        _axisPen.Freeze();
    }
    
    private void OnConfigChanged()
    {
        BuildPens();
        InvalidateVisual();
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth;
        double h = ActualHeight;
        
        var xExtentsPixels = _viewport.ToPixels(_viewport.SheetSize.X / 2);
        var yExtentsPixels = _viewport.ToPixels(_viewport.SheetSize.Y / 2);
        
        var origin = _viewport.ToPoint(Unit2D.Zero);

        var pageRect = new Rect(origin.X - xExtentsPixels,
                                origin.Y - yExtentsPixels,
                                xExtentsPixels * 2,
                                yExtentsPixels * 2);

        // Draw the physical paper background
        dc.DrawRectangle(Brushes.White, _pageOutlinePen, pageRect);
        
        if (!ShowGrid)
        {
            return;
        }

        // Clip everything else (grid/axes) to the paper boundary
        dc.PushClip(new RectangleGeometry(pageRect));

        var config = _appConfigService.Config;

        var spacing = config.GridSpacingMetric;
        var subdivisions = config.GridSubdivisionsMetric;
        
        var majorSpacingPixels = _viewport.ToPixels(spacing);
        var minorSpacingPixels = _viewport.ToPixels(spacing / subdivisions);
        var minSpacingPixels = config.GridMinSpacingPx;
        
        if (minorSpacingPixels > minSpacingPixels)
        {
            for (double x = 0; x <= xExtentsPixels; x += minorSpacingPixels)
            {
                dc.DrawLine(_minorPen, new Point(origin.X + x, pageRect.Top), new Point(origin.X + x, pageRect.Bottom));
                dc.DrawLine(_minorPen, new Point(origin.X - x, pageRect.Top), new Point(origin.X - x, pageRect.Bottom));
            }

            for (double y = 0; y <= yExtentsPixels; y += minorSpacingPixels)
            {
                dc.DrawLine(_minorPen, new Point(pageRect.Left, origin.Y + y), new Point(pageRect.Right, origin.Y + y));
                dc.DrawLine(_minorPen, new Point(pageRect.Left, origin.Y - y), new Point(pageRect.Right, origin.Y - y));
            }
        }

        for (double x = 0; x <= xExtentsPixels; x += majorSpacingPixels)
        {
            dc.DrawLine(_majorPen, new Point(origin.X + x, pageRect.Top), new Point(origin.X + x, pageRect.Bottom));
            dc.DrawLine(_majorPen, new Point(origin.X - x, pageRect.Top), new Point(origin.X - x, pageRect.Bottom));
        }

        for (double y = 0; y <= yExtentsPixels; y += majorSpacingPixels)
        {
            dc.DrawLine(_majorPen, new Point(pageRect.Left, origin.Y + y), new Point(pageRect.Right, origin.Y + y));
            dc.DrawLine(_majorPen, new Point(pageRect.Left, origin.Y - y), new Point(pageRect.Right, origin.Y - y));
        }

        dc.DrawLine(_axisPen, new Point(origin.X, pageRect.Top), new Point(origin.X, pageRect.Bottom));
        dc.DrawLine(_axisPen, new Point(pageRect.Left, origin.Y), new Point(pageRect.Right, origin.Y));

        dc.Pop();
    }

    public Unit2D? UnitSnap(Unit2D point, IUnitSnapContext context)
    {
        var config = _appConfigService.Config;

        var spacing = config.GridSpacingMetric;
        var subdivisions = config.GridSubdivisionsMetric;
        var minSpacingPixels = config.GridMinSpacingPx;

        var majorSpacing = spacing;
        var minorSpacing = spacing / subdivisions;
        
        var majorSnap = Unit2D.Snap(point, majorSpacing);
        var minorSnap = Unit2D.Snap(point, minorSpacing);
        var snap = point;

        bool hasMinorSpacing = _viewport.ToPixels(minorSpacing) > minSpacingPixels;
        
        if (!hasMinorSpacing || (point - majorSnap).SqrMagnitude < (point - minorSnap).SqrMagnitude)
        {
            snap = majorSnap;
        }
        else
        {
            snap = minorSnap;
        }
        
        return snap;
    }
}
