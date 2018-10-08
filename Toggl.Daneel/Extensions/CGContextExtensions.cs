using CoreGraphics;

namespace Toggl.Daneel.Extensions
{
    public static class CGContextExtensions
    {
        public static void DrawTokenShape(
            this CGContext context,
            CGPath tokenPath,
            CGColor foregroundColor,
            CGColor backgroundColor)
        {
            context.AddPath(tokenPath);
            context.SetFillColor(backgroundColor);
            context.FillPath();

            context.AddPath(tokenPath);
            context.SetStrokeColor(foregroundColor);
            context.StrokePath();
        }

        public static void DrawColoredImage(
            this CGContext context,
            CGRect rect,
            CGImage image,
            CGColor color)
        {
            try
            {
                // prevent the image from being flipped vertically
                context.TranslateCTM(0, 2 * rect.Y + rect.Height);
                context.ScaleCTM(1.0f, -1.0f);

                context.ClipToMask(rect, image);
                context.SetFillColor(color);
                context.FillRect(rect);
            }
            finally
            {
                // reset translation and scaling in the global context
                context.TranslateCTM(0.0f, 0.0f);
                context.ScaleCTM(1.0f, 1.0f);
            }
        }
    }
}
