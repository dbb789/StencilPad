using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.UI;

public class CanvasGrid : ContentControl, IUnitSnap
{
    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
    
    public static readonly DependencyProperty MinorSpacingProperty =
        DependencyProperty.Register(nameof(MinorSpacing), typeof(Unit), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(1m), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MajorSpacingProperty =
        DependencyProperty.Register(nameof(MajorSpacing), typeof(Unit), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(10), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MinorBrushProperty =
        DependencyProperty.Register(nameof(MinorBrush), typeof(Brush), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(64, 0, 128, 255)),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MajorBrushProperty =
        DependencyProperty.Register(nameof(MajorBrush), typeof(Brush), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 0, 128, 255)),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AxisBrushProperty =
        DependencyProperty.Register(nameof(AxisBrush), typeof(Brush), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(210, 0, 128, 255)),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MinimumSpacingPixelsProperty =
        DependencyProperty.Register(nameof(MinimumSpacingPixels), typeof(double), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }
    
    public Unit MinorSpacing
    {
        get => (Unit)GetValue(MinorSpacingProperty);
        set => SetValue(MinorSpacingProperty, value);
    }

    public Unit MajorSpacing
    {
        get => (Unit)GetValue(MajorSpacingProperty);
        set => SetValue(MajorSpacingProperty, value);
    }

    public Brush MinorBrush
    {
        get => (Brush)GetValue(MinorBrushProperty);
        set => SetValue(MinorBrushProperty, value);
    }

    public Brush MajorBrush
    {
        get => (Brush)GetValue(MajorBrushProperty);
        set => SetValue(MajorBrushProperty, value);
    }

    public Brush AxisBrush
    {
        get => (Brush)GetValue(AxisBrushProperty);
        set => SetValue(AxisBrushProperty, value);
    }
    
    public double MinimumSpacingPixels
    {
        get => (double)GetValue(MinimumSpacingPixelsProperty);
        set => SetValue(MinimumSpacingPixelsProperty, value);
    }

    private readonly IViewport _viewport;

    private Pen _pageOutlinePen = null!;
    private Pen _minorPen = null!;
    private Pen _majorPen = null!;
    private Pen _axisPen = null!;
    
    public CanvasGrid(IViewport viewport)
    {
        _viewport = viewport;

        _pageOutlinePen = new Pen(Brushes.LightGray, 1) { DashStyle = DashStyles.Solid };
        _pageOutlinePen.Freeze();
        
        _minorPen = new Pen(MinorBrush, 0.5) { DashStyle = DashStyles.Solid };
        _minorPen.Freeze();
        
        _majorPen = new Pen(MajorBrush, 0.5) { DashStyle = DashStyles.Solid };
        _majorPen.Freeze();
        
        _axisPen  = new Pen(AxisBrush, 1) { DashStyle = DashStyles.Solid };
        _axisPen.Freeze();
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth;
        double h = ActualHeight;
        
        var xExtentsPixels = _viewport.ToPixels(_viewport.Size.X / 2);
        var yExtentsPixels = _viewport.ToPixels(_viewport.Size.Y / 2);
        var xGridMinPixels = Math.Max(0, (w / 2) - xExtentsPixels);
        var yGridMinPixels = Math.Max(0, (h / 2) - yExtentsPixels);
        var xGridMaxPixels = w - xGridMinPixels;
        var yGridMaxPixels = h - yGridMinPixels;

        var pageRect = new Rect(xGridMinPixels,
                                yGridMinPixels,
                                xGridMaxPixels - xGridMinPixels,
                                yGridMaxPixels - yGridMinPixels);

        dc.DrawRectangle(Brushes.Transparent, _pageOutlinePen, pageRect);
        
        if (!ShowGrid)
        {
            return;
        }
        
        var origin = _viewport.ToPoint(Unit2D.Zero);
        var minorSpacingPixels = _viewport.ToPixels(MinorSpacing);
        var majorSpacingPixels = _viewport.ToPixels(MajorSpacing);

        if (_viewport.ToPixels(MinorSpacing) > MinimumSpacingPixels)
        {
            for (double x = 0; x <= xExtentsPixels; x += minorSpacingPixels)
            {
                dc.DrawLine(_minorPen, new Point(origin.X + x, yGridMinPixels), new Point(origin.X + x, yGridMaxPixels));
                dc.DrawLine(_minorPen, new Point(origin.X - x, yGridMinPixels), new Point(origin.X - x, yGridMaxPixels));
            }

            for (double y = 0; y <= yExtentsPixels; y += minorSpacingPixels)
            {
                dc.DrawLine(_minorPen, new Point(xGridMinPixels, origin.Y + y), new Point(xGridMaxPixels, origin.Y + y));
                dc.DrawLine(_minorPen, new Point(xGridMinPixels, origin.Y - y), new Point(xGridMaxPixels, origin.Y - y));
            }
        }

        for (double x = 0; x <= xExtentsPixels; x += majorSpacingPixels)
        {
            dc.DrawLine(_majorPen, new Point(origin.X + x, yGridMinPixels), new Point(origin.X + x, yGridMaxPixels));
            dc.DrawLine(_majorPen, new Point(origin.X - x, yGridMinPixels), new Point(origin.X - x, yGridMaxPixels));
        }

        for (double y = 0; y <= yExtentsPixels; y += majorSpacingPixels)
        {
            dc.DrawLine(_majorPen, new Point(xGridMinPixels, origin.Y + y), new Point(xGridMaxPixels, origin.Y + y));
            dc.DrawLine(_majorPen, new Point(xGridMinPixels, origin.Y - y), new Point(xGridMaxPixels, origin.Y - y));
        }

        dc.DrawLine(_axisPen, new Point(origin.X, yGridMinPixels), new Point(origin.X, yGridMaxPixels));
        dc.DrawLine(_axisPen, new Point(xGridMinPixels, origin.Y), new Point(xGridMaxPixels, origin.Y));
    }

    public Unit2D UnitSnap(Unit2D point, Handle? selfHandle = null)
    {
        var majorSnapX = Unit.FromMillimeters(Math.Round(point.X.Millimeters / MajorSpacing.Millimeters) * MajorSpacing.Millimeters);
        var majorSnapY = Unit.FromMillimeters(Math.Round(point.Y.Millimeters / MajorSpacing.Millimeters) * MajorSpacing.Millimeters);
        var minorSnapX = Unit.FromMillimeters(Math.Round(point.X.Millimeters / MinorSpacing.Millimeters) * MinorSpacing.Millimeters);
        var minorSnapY = Unit.FromMillimeters(Math.Round(point.Y.Millimeters / MinorSpacing.Millimeters) * MinorSpacing.Millimeters);

        Unit snapX = point.X;
        Unit snapY = point.Y;

        bool hasMinorSpacing = _viewport.ToPixels(MinorSpacing) > MinimumSpacingPixels;
        
        if (!hasMinorSpacing ||
            Math.Abs(point.X.Millimeters - majorSnapX.Millimeters) < Math.Abs(point.X.Millimeters - minorSnapX.Millimeters))
        {
            snapX = majorSnapX;
        }
        else
        {
            snapX = minorSnapX;
        }
        
        if (!hasMinorSpacing ||
            Math.Abs(point.Y.Millimeters - majorSnapY.Millimeters) < Math.Abs(point.Y.Millimeters - minorSnapY.Millimeters))
        {
            snapY = majorSnapY;
        }
        else
        {
            snapY = minorSnapY;
        }
        
        return new Unit2D(snapX, snapY);
    }
}
