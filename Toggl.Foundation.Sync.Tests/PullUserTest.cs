using System.Linq;
using FluentAssertions;
using Toggl.Foundation.Sync.Tests.Extensions;
using Toggl.Foundation.Sync.Tests.Helpers;
using Toggl.Foundation.Sync.Tests.State;

namespace Toggl.Foundation.Sync.Tests
{
    public sealed class PullUserTest : BaseComplexSyncTest
    {
        protected override ServerState ArrangeServerState(ServerState initialServerState)
        {
            return initialServerState;
        }

        protected override DatabaseState ArrangeDatabaseState(ServerState serverState)
        {
            return new DatabaseState(
                user: serverState.User.With(defaultWorkspaceId: null).ToSyncable(),
                preferences: serverState.Preferences.ToSyncable(),
                workspaces: serverState.Workspaces.Select(ws => ws.ToSyncable()));
        }

        protected override void AssertFinalState(AppServices services, ServerState finalServerState, DatabaseState finalDatabaseState)
        {
            finalDatabaseState.User.DefaultWorkspaceId.Should().NotBeNull();
            finalDatabaseState.User.DefaultWorkspaceId.Should().Be(finalServerState.User.DefaultWorkspaceId);
        }
    }
}
