using System;
using Android.Views;

namespace Toggl.Giskard.Extensions
{
    public static class VisibilityExtensions
    {
        public static bool ToBool(this ViewStates state)
            => state == ViewStates.Visible;

        public static ViewStates ToVisibility(this bool state, bool useGone = true)
            => state ? ViewStates.Visible : (useGone ? ViewStates.Gone : ViewStates.Invisible);

        public static void BeVisible(this View view, bool visible)
        {
            if (view != null)
            {
                view.Visibility = ToVisibility(visible);
            }
        }
    }
}
