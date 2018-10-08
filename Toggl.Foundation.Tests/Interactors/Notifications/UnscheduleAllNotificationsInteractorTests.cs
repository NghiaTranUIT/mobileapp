using NSubstitute;
using Toggl.Foundation.Interactors.Notifications;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors.Notifications
{
    public class UnscheduleAllNotificationsInteractorTests
    {
        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private readonly UnscheduleAllNotificationsInteractor interactor;

            public TheExecuteMethod()
            {
                interactor = new UnscheduleAllNotificationsInteractor(NotificationService);
            }

            [Fact, LogIfTooSlow]
            public void UnschedulesAllNotifications()
            {
                interactor.Execute();

                NotificationService.Received().UnscheduleAllNotifications();
            }
        }
    }
}
