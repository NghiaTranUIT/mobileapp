using System;
using System.Reactive.Linq;
using MvvmCross;
using Toggl.Foundation.MvvmCross.Services;

namespace Toggl.Giskard.Services
{
    [Preserve(AllMembers = true)]
    public sealed class PermissionsServiceAndroid : IPermissionsService
    {
        public IObservable<bool> CalendarPermissionGranted
            => Observable.Return(false);

        public IObservable<bool> NotificationPermissionGranted
            => Observable.Return(false);

        public IObservable<bool> RequestCalendarAuthorization(bool force = false)
            => Observable.Return(false);

        public IObservable<bool> RequestNotificationAuthorization(bool force = false)
            => Observable.Return(false);

        public void OpenAppSettings()
        {
        }
    }
}
