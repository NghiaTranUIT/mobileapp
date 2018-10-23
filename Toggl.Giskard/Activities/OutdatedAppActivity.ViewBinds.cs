using Android.Widget;

namespace Toggl.Giskard.Activities
{
    public sealed partial class OutdatedAppActivity
    {
        private Button updateAppButton;
        private Button openWebsiteButton;

        private void initializeViews()
        {
            updateAppButton = FindViewById<Button>(Resource.Id.UpdateAppButton);
            openWebsiteButton = FindViewById<Button>(Resource.Id.OpenWebsiteButton);
        }
    }
}