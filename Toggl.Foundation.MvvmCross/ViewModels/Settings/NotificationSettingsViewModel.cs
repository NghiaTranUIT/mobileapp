using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.MvvmCross.Extensions;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels.Settings
{
    [Preserve(AllMembers = true)]
    public sealed class NotificationSettingsViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService navigationService;
        private readonly IBackgroundService backgroundService;
        private readonly IPermissionsService permissionsService;
        private readonly IUserPreferences userPreferences;

        private readonly ISubject<bool> permissionGrantedSubject = new BehaviorSubject<bool>(false);
        private readonly ISubject<string> upcomingEventSubject = new BehaviorSubject<string>(Resources.Disabled);

        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        public IObservable<bool> PermissionGranted => permissionGrantedSubject.AsObservable().DistinctUntilChanged();

        public IObservable<string> UpcomingEvents => upcomingEventSubject.AsObservable().DistinctUntilChanged();

        public UIAction RequestAccess { get; }

        public UIAction OpenUpcomingEvents { get; }

        public NotificationSettingsViewModel(
            IMvxNavigationService navigationService,
            IBackgroundService backgroundService,
            IPermissionsService permissionsService,
            IUserPreferences userPreferences)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(backgroundService, nameof(backgroundService));
            Ensure.Argument.IsNotNull(permissionsService, nameof(permissionsService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));

            this.navigationService = navigationService;
            this.backgroundService = backgroundService;
            this.permissionsService = permissionsService;
            this.userPreferences = userPreferences;

            backgroundService
                .AppResumedFromBackground
                .SelectUnit()
                .Subscribe(refreshPermissionGranted)
                .DisposedBy(disposeBag);

            RequestAccess = UIAction.FromAction(requestAccess);
            OpenUpcomingEvents = UIAction.FromAsync(openUpcomingEvents);
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await refreshPermissionGranted();
            await refreshUpcomingEventsValue();
        }

        private void requestAccess()
        {
            permissionsService.OpenAppSettings();
        }

        private async Task openUpcomingEvents()
        {
            await navigationService.Navigate<UpcomingEventsNotificationSettingsViewModel, Unit>();
            await refreshUpcomingEventsValue();
        }

        private async Task refreshPermissionGranted()
        {
            var permissionGranted = await permissionsService.NotificationPermissionGranted.FirstAsync();
            permissionGrantedSubject.OnNext(permissionGranted);
        }

        private async Task refreshUpcomingEventsValue()
        {
            var calendarNotificationsOption = await userPreferences.CalendarNotificationsSettings().FirstAsync();
            upcomingEventSubject.OnNext(calendarNotificationsOption.Title());
        }
    }
}
