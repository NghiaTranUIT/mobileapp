using System;
using System.Reactive;
using Toggl.Foundation.Interactors.Notifications;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<Unit>> UnscheduleAllNotifications()
            => new UnscheduleAllNotificationsInteractor(notificationService);

        public IInteractor<IObservable<Unit>> ScheduleEventNotificationsForNextWeek()
            => new ScheduleEventNotificationsInteractor(timeService, calendarService, userPreferences, notificationService);
    }
}
