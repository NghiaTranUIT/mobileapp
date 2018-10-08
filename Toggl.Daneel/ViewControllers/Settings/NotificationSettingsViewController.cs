using MvvmCross.Platforms.Ios.Presenters.Attributes;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Settings;
using Toggl.Multivac.Extensions;
using FoundationResources = Toggl.Foundation.Resources;

namespace Toggl.Daneel.ViewControllers.Settings
{
    [MvxChildPresentation]
    public sealed partial class NotificationSettingsViewController : ReactiveViewController<NotificationSettingsViewModel>
    {
        public NotificationSettingsViewController() : base(nameof(NotificationSettingsViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.Title = FoundationResources.NotificationSettingsTitle;

            this.Bind(ViewModel.PermissionGranted.Invert(), OpenSettingsContainer.Rx().IsVisible());
            this.Bind(ViewModel.PermissionGranted, CalendarNotificationsContainer.Rx().IsVisible());

            this.Bind(OpenSettingsButton.Rx().Tap(), ViewModel.RequestAccessAction);

            this.Bind(CalendarNotificationsRow.Rx().Tap(), ViewModel.OpenUpcomingEvents);
            this.Bind(ViewModel.UpcomingEvents, CalendarNotificationsValue.Rx().Text());
        }
    }
}
