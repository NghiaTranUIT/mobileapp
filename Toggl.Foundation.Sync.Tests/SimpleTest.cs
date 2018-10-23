using FluentAssertions;
using Toggl.Foundation.Sync.Tests.Extensions;
using Toggl.Foundation.Sync.Tests.Helpers;
using Toggl.Foundation.Sync.Tests.State;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.Tests
{
    public sealed class SimpleTest : BaseComplexSyncTest
    {
        protected override ServerState ArrangeServerState(ServerState initialServerState)
        {
            return initialServerState;
        }

        protected override DatabaseState ArrangeDatabaseState(ServerState serverState)
        {
            return new DatabaseState(
                user: serverState.User.ToSyncable(
                    syncStatus: SyncStatus.SyncNeeded,
                    at: serverState.User.At.AddMinutes(-10),
                    email: Email.From("this@email.com")));
        }

        protected override void AssertFinalState(
            AppServices appServices, ServerState finalServerState, DatabaseState finalDatabaseState)
        {
            finalServerState.User.Email.Should().NotBe("this@email.com");
            finalDatabaseState.User.Email.Should().Be(finalServerState.User.Email);
        }
    }
}
