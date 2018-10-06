using System;
using CoreGraphics;
using Foundation;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Services;
using UIKit;

namespace Toggl.Daneel.Autocomplete
{
    public abstract class ImageTokenTextAttachment : TokenTextAttachment
    {
        protected ImageTokenTextAttachment(
            NSAttributedString stringToDraw,
            nfloat textVerticalOffset,
            UIColor foregroundColor,
            UIColor backgroundColor,
            int imageWidth,
            int imageHeight,
            UIImage image,
            nfloat fontDescender)
            : base(fontDescender)
        {
            var size = CalculateSize(stringToDraw, imageWidth + TokenPadding);

            UIGraphics.BeginImageContextWithOptions(size, opaque: false, scale: 0.0f);
            using (var context = UIGraphics.GetCurrentContext())
            {
                var tokenPath = CalculateTokenPath(size);
                context.DrawTokenShape(
                    tokenPath.CGPath, foregroundColor.CGColor, backgroundColor.CGColor);

                stringToDraw.DrawString(
                    new CGPoint(
                        x: TokenMargin + TokenPadding + imageWidth + TokenPadding,
                        y: textVerticalOffset));

                var rect = new CGRect(
                    x: TokenMargin + TokenPadding,
                    y: (size.Height - imageHeight) / 2,
                    width: imageWidth,
                    height: imageHeight);

                context.DrawColoredImage(rect, image.CGImage, foregroundColor.CGColor);

                var finalImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                Image = finalImage;
            }
        }
    }
}
