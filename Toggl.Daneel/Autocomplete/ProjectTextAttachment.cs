using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Toggl.Daneel.Autocomplete
{
    public sealed class ProjectTextAttachment : ImageTokenTextAttachment
    {
        private const int imageWidth = 12;

        private const int imageHeight = 9;

        private static nfloat backgroundAlpha = 0.12f;

        private static UIImage image => UIImage.FromBundle("icProjects");

        public ProjectTextAttachment(
            NSAttributedString stringToDraw,
            UIColor projectColor,
            nfloat textVerticalOffset,
            nfloat fontDescender)
            : base(
                stringToDraw,
                textVerticalOffset,
                projectColor,
                projectColor.ColorWithAlpha(backgroundAlpha),
                imageWidth,
                imageHeight,
                image,
                fontDescender)
        {
        }
    }
}
