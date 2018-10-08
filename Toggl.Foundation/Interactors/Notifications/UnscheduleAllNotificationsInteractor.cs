using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Services;
using Toggl.Multivac;

namespace Toggl.Foundation.Interactors.Notifications
{
    public class UnscheduleAllNotificationsInteractor : IInteractor<IObservable<Unit>>
    {
        private readonly INotificationService notificationService;

        public UnscheduleAllNotificationsInteractor(INotificationService notificationService)
        {
            Ensure.Argument.IsNotNull(notificationService, nameof(notificationService));

            this.notificationService = notificationService;
        }

        public IObservable<Unit> Execute()
            => notificationService
                .UnscheduleAllNotifications()
                .Catch<Unit, NotAuthorizedException>(
                    ex => Observable.Return(Unit.Default)
                );
    }
}
