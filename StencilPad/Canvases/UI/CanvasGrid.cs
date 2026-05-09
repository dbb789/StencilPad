using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Spatial;

namespace StencilPad.Canvases.UI;

public class CanvasGrid : ContentControl, IUnitSnap
{
    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
    
    public static readonly DependencyProperty MinorSpacingProperty =
        DependencyProperty.Register(nameof(MinorSpacing), typeof(Unit), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(1.0), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MajorSpacingProperty =
        DependencyProperty.Register(nameof(MajorSpacing), typeof(Unit), typeof(CanvasGrid),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(10.0), FrameworkPropertyMetadataOptions.AffectsRender));

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

    public CanvasGrid(IViewport viewport)
    {
        _viewport = viewport;
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        if (!ShowGrid)
        {
            return;
        }
        
        var minorPen = new Pen(MinorBrush, 0.5) { DashStyle = DashStyles.Solid };
        var majorPen = new Pen(MajorBrush, 0.5) { DashStyle = DashStyles.Solid };
        var axisPen  = new Pen(AxisBrush,  1) { DashStyle = DashStyles.Solid };

        minorPen.Freeze();
        majorPen.Freeze();
        axisPen.Freeze();

        double w = ActualWidth;
        double h = ActualHeight;
        var origin = _viewport.ToPoint(Unit2D.Zero);

        if (_viewport.ToPixels(MinorSpacing) > MinimumSpacingPixels)
        {
            for (var x = Unit.Zero; x.Millimeters <= _viewport.Size.X.Millimeters / 2.0; x += MinorSpacing)
            {
                double px = _viewport.ToPixels(x);

                dc.DrawLine(minorPen, new Point(origin.X + px, 0), new Point(origin.X + px, h));
                dc.DrawLine(minorPen, new Point(origin.X - px, 0), new Point(origin.X - px, h));
            }

            for (var y = Unit.Zero; y.Millimeters <= _viewport.Size.Y.Millimeters / 2.0; y += MinorSpacing)
            {
                double py = _viewport.ToPixels(y);

                dc.DrawLine(minorPen, new Point(0, origin.Y + py), new Point(w, origin.Y + py));
                dc.DrawLine(minorPen, new Point(0, origin.Y - py), new Point(w, origin.Y - py));
            }
        }

        for (var x = Unit.Zero; x.Millimeters <= _viewport.Size.X.Millimeters / 2.0; x += MajorSpacing)
        {
            double px = _viewport.ToPixels(x);

            dc.DrawLine(majorPen, new Point(origin.X + px, 0), new Point(origin.X + px, h));
            dc.DrawLine(majorPen, new Point(origin.X - px, 0), new Point(origin.X - px, h));
        }

        for (var y = Unit.Zero; y.Millimeters <= _viewport.Size.Y.Millimeters / 2.0; y += MajorSpacing)
        {
            double py = _viewport.ToPixels(y);

            dc.DrawLine(majorPen, new Point(0, origin.Y + py), new Point(w, origin.Y + py));
            dc.DrawLine(majorPen, new Point(0, origin.Y - py), new Point(w, origin.Y - py));
        }

        dc.DrawLine(axisPen, new Point(origin.X, 0), new Point(origin.X, h));
        dc.DrawLine(axisPen, new Point(0, origin.Y), new Point(w, origin.Y));
    }

    public Unit2D UnitSnap(Unit2D point)
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
