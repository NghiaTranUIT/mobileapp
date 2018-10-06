using System;
using Foundation;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Autocomplete
{
    public sealed class TagTextAttachment : ImageTokenTextAttachment
    {
        private static readonly UIColor borderColor = Color.StartTimeEntry.TokenBorder.ToNativeColor();

        private const int tagImageWidth = 12;

        private const int tagImageHeight = 12;

        private static UIImage image => UIImage.FromBundle("icTags");

        public TagTextAttachment(
            NSAttributedString stringToDraw,
            nfloat textVerticalOffset,
            nfloat fontDescender)
            : base(
                stringToDraw,
                textVerticalOffset,
                borderColor,
                borderColor.ColorWithAlpha(0.12f),
                tagImageWidth,
                tagImageHeight,
                image,
                fontDescender)
        {
        }
    }
}
