#if IOS
using CoreGraphics;
using UIKit;
#endif

namespace Plugin.ContextMenu;

public class RoundedRectanglePreviewProvider : IContextMenuPreviewProvider
{
    public Thickness CornerRadius { get; set; }

#if IOS
    public UITargetedPreview GetPreviewForHighlightingMenu(UIContextMenuInteraction interaction, UIContextMenuConfiguration configuration)
    {
        var parameters = new UIKit.UIPreviewParameters();
        parameters.BackgroundColor = UIColor.Clear;
        parameters.VisiblePath = GetVisiblePath(interaction.View.Frame);

        return new UITargetedPreview(interaction.View, parameters);
    }

    UIBezierPath GetVisiblePath(CGRect rect)
    {
        var minx = rect.GetMinX();
        var miny = rect.GetMinY();
        var maxx = rect.GetMaxX();
        var maxy = rect.GetMaxY();

        float M_PI = (float)Math.PI;
        float M_PI_2 = (float)Math.PI * 2;

        var path = new UIBezierPath();
        path.MoveTo(new CGPoint(minx + CornerRadius.Left, miny));
        path.AddLineTo(new CGPoint(maxx - CornerRadius.Top, miny));
        path.AddArc(new CGPoint(maxx - CornerRadius.Top, miny + CornerRadius.Top), radius: (float)CornerRadius.Top, startAngle: 3 * M_PI_2, endAngle: 0, clockWise: true);
        path.AddLineTo(new CGPoint(maxx, maxy - CornerRadius.Right));
        path.AddArc(new CGPoint(maxx - CornerRadius.Right, maxy - CornerRadius.Right), radius: (float)CornerRadius.Right, startAngle: 0, endAngle: M_PI_2, clockWise: true);
        path.AddLineTo(new CGPoint(minx + CornerRadius.Bottom, maxy));
        path.AddArc(new CGPoint(minx + CornerRadius.Bottom, maxy - CornerRadius.Bottom), radius: (float)CornerRadius.Bottom, startAngle: M_PI_2, endAngle: M_PI, clockWise: true);
        path.AddLineTo(new CGPoint(minx, miny + CornerRadius.Left));
        path.AddArc(new CGPoint(minx + CornerRadius.Left, miny + CornerRadius.Left), radius: (float)CornerRadius.Left, startAngle: M_PI, endAngle: 3 * M_PI_2, clockWise: true);
        path.ClosePath();

        return path;
    }
#endif
}