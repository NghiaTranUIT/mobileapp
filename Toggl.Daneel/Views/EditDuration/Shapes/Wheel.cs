using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;
using static Toggl.Multivac.Math;

namespace Toggl.Daneel.Views.EditDuration.Shapes
{
    public sealed class Wheel : CAShapeLayer
    {
        public Wheel(CGPoint center, nfloat outerRadius, nfloat innerRadius, CGColor background)
        {
            var discPath = new UIBezierPath();
            discPath.AddArc(center, outerRadius, 0, (nfloat)FullCircle, true);
            var cutOutPath = new UIBezierPath();
            cutOutPath.AddArc(center, innerRadius, 0, (nfloat)FullCircle, true);
            discPath.AppendPath(cutOutPath.BezierPathByReversingPath());

            Path = discPath.CGPath;
            FillColor = background;
        }
    }
}
