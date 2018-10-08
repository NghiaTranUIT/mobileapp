using System;
using System.Collections.Immutable;
using System.Reactive;
using Notification = Toggl.Multivac.Notification;

namespace Toggl.Foundation.Services
{
    public interface INotificationService
    {
        IObservable<Unit> Schedule(IImmutableList<Notification> notifications);
        IObservable<Unit> UnscheduleAllNotifications();
    }
}
