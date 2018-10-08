using System;
using System.Reactive.Linq;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;
using UIKit;
using Math = System.Math;

namespace Toggl.Daneel.ViewControllers
{
    [MvxChildPresentation]
    public partial class SettingsViewController : ReactiveViewController<SettingsViewModel>
    {
        private const int verticalSpacing = 24;

        public SettingsViewController()
            : base(nameof(SettingsViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            Title = ViewModel.Title;
            VersionLabel.Text = ViewModel.Version;

            LoggingOutView.Hidden = true;
            SendFeedbackSuccessView.Hidden = true;

            this.Bind(ViewModel.Email, EmailLabel.Rx().Text());
            this.Bind(ViewModel.IsSynced, SyncedView.Rx().IsVisible());
            this.Bind(ViewModel.WorkspaceName, WorkspaceLabel.Rx().Text());
            this.Bind(ViewModel.DurationFormat, DurationFormatLabel.Rx().Text());
            this.Bind(ViewModel.IsRunningSync, SyncingView.Rx().IsVisible());
            this.Bind(ViewModel.DateFormat, DateFormatLabel.Rx().Text());
            this.Bind(ViewModel.BeginningOfWeek, BeginningOfWeekLabel.Rx().Text());
            this.Bind(ViewModel.IsFeedbackSuccessViewShowing, SendFeedbackSuccessView.Rx().AnimatedIsVisible());
            this.BindVoid(ViewModel.LoggingOut, () =>
            {
                LoggingOutView.Hidden = false;
                SyncingView.Hidden = true;
                SyncedView.Hidden = true;
            });

            this.Bind(HelpView.Rx().Tap(), ViewModel.OpenHelpView);
            this.Bind(LogoutButton.Rx().Tap(), ViewModel.TryLogout);
            this.Bind(AboutView.Rx().Tap(), ViewModel.OpenAboutView);
            this.Bind(FeedbackView.Rx().Tap(), ViewModel.SubmitFeedback);
            this.Bind(DateFormatView.Rx().Tap(), ViewModel.SelectDateFormat);
            this.Bind(WorkspaceView.Rx().Tap(), ViewModel.PickDefaultWorkspace);
            this.Bind(DurationFormatView.Rx().Tap(), ViewModel.SelectDurationFormat);
            this.BindVoid(ManualModeSwitch.Rx().Changed(), ViewModel.ToggleManualMode);
            this.Bind(BeginningOfWeekView.Rx().Tap(), ViewModel.SelectBeginningOfWeek);
            this.Bind(CalendarSettingsView.Rx().Tap(), ViewModel.OpenCalendarSettingsAction);
            this.BindVoid(SendFeedbackSuccessView.Rx().Tap(), ViewModel.CloseFeedbackSuccessView);
            this.Bind(NotificationSettingsView.Rx().Tap(), ViewModel.OpenNotificationSettingsAction);
            this.Bind(TwentyFourHourClockSwitch.Rx().Changed(), ViewModel.ToggleUseTwentyFourHourClock);

            UIApplication.Notifications
                .ObserveWillEnterForeground((sender, e) => startAnimations())
                .DisposedBy(DisposeBag);

            if (!ViewModel.CalendarSettingsEnabled)
                hideCalendarSettingsSection();

            ViewModel.IsManualModeEnabled
                .FirstAsync()
                .Subscribe(isEnabled => ManualModeSwitch.SetState(isEnabled, false))
                .DisposedBy(DisposeBag);

            ViewModel.UseTwentyFourHourFormat
                .FirstAsync()
                .Subscribe(useTwentyFourHourFormat => TwentyFourHourClockSwitch.SetState(useTwentyFourHourFormat, false))
                .DisposedBy(DisposeBag);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            startAnimations();
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            tryAlignLogoutButtonWithBottomEdge();
        }

        private void prepareViews()
        {
            // Syncing indicator colors
            setIndicatorSyncColor(SyncedIcon);
            setIndicatorSyncColor(SyncingIndicator);
            setIndicatorSyncColor(LoggingOutIndicator);
        }

        private void hideCalendarSettingsSection()
        {
            CalendarSettingsSection.Hidden = true;
            CalendarSectionTopConstraint.Constant = 0;
            CalendarSettingsSection.HeightAnchor.ConstraintEqualTo(0).Active = true;
        }

        private void setIndicatorSyncColor(UIImageView imageView)
        {
            imageView.Image = imageView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            imageView.TintColor = Color.Settings.SyncStatusText.ToNativeColor();
        }

        private void startAnimations()
        {
            SyncingActivityIndicatorView.StartSpinning();
            LoggingOutActivityIndicatorView.StartSpinning();
        }

        private void tryAlignLogoutButtonWithBottomEdge()
        {
            var contentHeight = LogoutContainerView.Frame.Top - LogoutVerticalOffsetConstraint.Constant + LogoutContainerView.Frame.Height;
            var bottomOffset = verticalSpacing;
            var idealDistance = ScrollView.Frame.Height - contentHeight - bottomOffset;
            var distance = Math.Max(idealDistance, verticalSpacing);
            LogoutVerticalOffsetConstraint.Constant = (nfloat)distance;
        }
    }
}
