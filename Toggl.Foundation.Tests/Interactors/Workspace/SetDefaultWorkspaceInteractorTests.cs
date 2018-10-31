using System;
using System.Reactive.Linq;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors.Workspace
{
    public sealed class SetDefaultWorkspaceInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useTimeService, bool useDataSource)
            {
                Action tryingToConstructWithNull = () => new SetDefaultWorkspaceInteractor(
                    useTimeService ? TimeService : null,
                    useDataSource ? DataSource.User : null,
                    12
                );

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            public TheExecuteMethod()
            {
                var user = new MockUser
                {
                    Id = 666,
                    Email = Email.From("valid@email.com"),
                    BeginningOfWeek = BeginningOfWeek.Monday,
                    Fullname = "Full Name",
                    DefaultWorkspaceId = null
                };
                DataSource.User.Current.Returns(Observable.Return(user));
            }

            [Property, LogIfTooSlow]
            public void SetsTheDefaultWorkspaceId(long defaultWorkspaceId)
            {
                var interactor = new SetDefaultWorkspaceInteractor(TimeService, DataSource.User, defaultWorkspaceId);

                interactor.Execute().Wait();

                DataSource.User.Received().Update(Arg.Is<IThreadSafeUser>(user => user.DefaultWorkspaceId == defaultWorkspaceId)).Wait();
            }

            [Property, LogIfTooSlow]
            public void SetsSyncStatusToSyncNeeded(long defaultWorkspaceId)
            {
                var interactor = new SetDefaultWorkspaceInteractor(TimeService, DataSource.User, defaultWorkspaceId);

                interactor.Execute().Wait();

                DataSource.User.Received().Update(Arg.Is<IThreadSafeUser>(user => user.SyncStatus == SyncStatus.SyncNeeded)).Wait();
            }

            [Property, LogIfTooSlow]
            public void SetsAtToCurrentTime(DateTimeOffset now)
            {
                TimeService.CurrentDateTime.Returns(now);
                var interactor = new SetDefaultWorkspaceInteractor(TimeService, DataSource.User, 12);

                interactor.Execute().Wait();

                DataSource.User.Received().Update(Arg.Is<IThreadSafeUser>(user => user.At == now)).Wait();
            }
        }
    }
}
