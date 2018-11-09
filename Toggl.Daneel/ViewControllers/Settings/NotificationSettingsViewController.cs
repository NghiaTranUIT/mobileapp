using System;
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

            ViewModel.PermissionGranted
                .Invert()
                .Subscribe(OpenSettingsContainer.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            ViewModel.PermissionGranted
                .Subscribe(CalendarNotificationsContainer.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            OpenSettingsButton.Rx()
                .BindAction(ViewModel.RequestAccess)
                .DisposedBy(DisposeBag);

            CalendarNotificationsRow.Rx().Tap()
                .Subscribe(ViewModel.OpenUpcomingEvents.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.UpcomingEvents
                .Subscribe(CalendarNotificationsValue.Rx().Text())
                .DisposedBy(DisposeBag);
        }
    }
}
