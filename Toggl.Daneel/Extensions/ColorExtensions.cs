using System;
using CoreGraphics;

namespace Toggl.Daneel.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHexColor(this CGColor cgColor)
        {
            var r = (int)(cgColor.Components[0] * 255);
            var g = (int)(cgColor.Components[1] * 255);
            var b = (int)(cgColor.Components[2] * 255);

            return $"#{r:X02}{g:X02}{b:X02}";
        }
    }
}
