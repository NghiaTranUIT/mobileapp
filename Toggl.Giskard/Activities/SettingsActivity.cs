using System.Reactive.Linq;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Extensions.Reactive;
using Toggl.Giskard.ViewHolders;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class SettingsActivity : ReactiveActivity<SettingsViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SettingsActivity);

            OverridePendingTransition(Resource.Animation.abc_slide_in_right, Resource.Animation.abc_fade_out);

            InitializeViews();

            var adapter = new SimpleAdapter<SelectableWorkspaceViewModel>(
                Resource.Layout.SettingsActivityWorkspaceCell,
                WorkspaceSelectionViewHolder.Create
            );
            adapter.OnItemTapped = ViewModel.SelectDefaultWorkspace;
            workspacesRecyclerView.SetAdapter(adapter);
            workspacesRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            versionTextView.Text = ViewModel.Version;

            this.Bind(ViewModel.Name, nameTextView.Rx().TextObserver());
            this.Bind(ViewModel.Email, emailTextView.Rx().TextObserver());
            this.Bind(ViewModel.Workspaces, adapter.Rx().Items());
            this.Bind(ViewModel.IsManualModeEnabled, manualModeSwitch.Rx().Checked());
            this.Bind(ViewModel.UseTwentyFourHourFormat, is24hoursModeSwitch.Rx().Checked());
            this.Bind(ViewModel.AreRunningTimerNotificationsEnabled, runningTimerNotificationsSwitch.Rx().Checked());
            this.Bind(ViewModel.AreStoppedTimerNotificationsEnabled, stoppedTimerNotificationsSwitch.Rx().Checked());
            this.Bind(ViewModel.BeginningOfWeek, beginningOfWeekTextView.Rx().TextObserver());
            this.Bind(ViewModel.UserAvatar.Select(userImageFromBytes), bitmap =>
            {
                avatarView.SetImageBitmap(bitmap);
                avatarContainer.Visibility = ViewStates.Visible;
            });
            
            this.BindVoid(ViewModel.LoggingOut, this.CancelAllNotifications);
            this.Bind(ViewModel.IsFeedbackSuccessViewShowing, showFeedbackSuccessToast);

            this.Bind(logoutView.Rx().Tap(), ViewModel.TryLogout);
            this.Bind(helpView.Rx().Tap(), ViewModel.OpenHelpView);
            this.Bind(aboutView.Rx().Tap(), ViewModel.OpenAboutView);
            this.Bind(feedbackView.Rx().Tap(), ViewModel.SubmitFeedback);
            this.BindVoid(manualModeView.Rx().Tap(), ViewModel.ToggleManualMode);
            this.Bind(is24hoursModeView.Rx().Tap(), ViewModel.ToggleTwentyFourHourSettings);
            this.BindVoid(runningTimerNotificationsView.Rx().Tap(), ViewModel.ToggleRunningTimerNotifications);
            this.BindVoid(stoppedTimerNotificationsView.Rx().Tap(), ViewModel.ToggleStoppedTimerNotifications);
            this.Bind(beginningOfWeekView.Rx().Tap(), ViewModel.SelectBeginningOfWeek);

            setupToolbar();
        }

        private void showFeedbackSuccessToast(bool succeeeded)
        {
            if (!succeeeded) return;

            var toast = Toast.MakeText(this, Resource.String.SendFeedbackSuccessMessage, ToastLength.Long);
            toast.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.Bottom, 0, 0);
            toast.Show();
        }

        private Bitmap userImageFromBytes(byte[] imageBytes)
            => BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);

        private void setupToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.Toolbar);

            toolbar.Title = ViewModel.Title;

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            toolbar.NavigationClick += onNavigateBack;
        }

        private void onNavigateBack(object sender, Toolbar.NavigationClickEventArgs e)
        {
            ViewModel.GoBack();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_right);
        }
    }
}
