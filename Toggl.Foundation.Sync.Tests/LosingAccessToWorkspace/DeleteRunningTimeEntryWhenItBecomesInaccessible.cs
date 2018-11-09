using System;
using System.Linq;
using FluentAssertions;
using Toggl.Foundation.Sync.Tests.Extensions;
using Toggl.Foundation.Sync.Tests.Helpers;
using Toggl.Foundation.Sync.Tests.State;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.Tests.LosingAccessToWorkspace
{
    public sealed class DeleteRunningTimeEntryWhenItBecomesInaccessible
    {
        public sealed class DeletesSyncedRunningTimeEntryWhenItBecomesInaccessible
            : BaseComplexSyncTest
        {
            protected override ServerState ArrangeServerState(ServerState initialServerState)
                => initialServerState;

            protected override DatabaseState ArrangeDatabaseState(ServerState serverState)
                => new DatabaseState(
                    user: serverState.User.ToSyncable(),
                    workspaces: new[]
                    {
                        serverState.Workspaces.Single().ToSyncable(),
                        new MockWorkspace { Id = 1, SyncStatus = SyncStatus.InSync, IsInaccessible = false }
                    },
                    timeEntries: new[]
                    {
                        new MockTimeEntry
                        {
                            Id = 2,
                            WorkspaceId = 1,
                            Start = DateTimeOffset.Now.AddHours(-1),
                            Duration = null,
                            SyncStatus = SyncStatus.InSync
                        },
                    });

            protected override void AssertFinalState(
                AppServices services, ServerState finalServerState, DatabaseState finalDatabaseState)
            {
                finalDatabaseState.TimeEntries.Should().HaveCount(0);
            }
        }

        public sealed class DoesNotDeleteUnsyncedTimeEntryWhenItBecomesInaccessible
            : BaseComplexSyncTest
        {
            protected override ServerState ArrangeServerState(ServerState initialServerState)
                => initialServerState;

            protected override DatabaseState ArrangeDatabaseState(ServerState serverState)
                => new DatabaseState(
                    user: serverState.User.ToSyncable(),
                    workspaces: new[]
                    {
                        serverState.Workspaces.Single().ToSyncable(),
                        new MockWorkspace { Id = 1, SyncStatus = SyncStatus.InSync, IsInaccessible = false }
                    },
                    timeEntries: new[]
                    {
                        new MockTimeEntry
                        {
                            Id = 2,
                            WorkspaceId = 1,
                            Start = DateTimeOffset.Now.AddHours(-1),
                            Duration = null,
                            SyncStatus = SyncStatus.SyncNeeded
                        },
                    });

            protected override void AssertFinalState(
                AppServices services, ServerState finalServerState, DatabaseState finalDatabaseState)
            {
                finalDatabaseState.TimeEntries.Should().HaveCount(1);
            }
        }

        public sealed class DoesNotDeleteSyncedRunningTimeEntryWhenADifferentWorkspaceBecomesInaccessible
            : BaseComplexSyncTest
        {
            protected override ServerState ArrangeServerState(ServerState initialServerState)
                => initialServerState;

            protected override DatabaseState ArrangeDatabaseState(ServerState serverState)
                => new DatabaseState(
                    user: serverState.User.ToSyncable(),
                    workspaces: new[]
                    {
                        serverState.Workspaces.Single().ToSyncable(),
                        new MockWorkspace { Id = 1, SyncStatus = SyncStatus.InSync, IsInaccessible = false }
                    },
                    timeEntries: new[]
                    {
                        new MockTimeEntry
                        {
                            Id = 2,
                            WorkspaceId = serverState.Workspaces.Single().Id,
                            Start = DateTimeOffset.Now.AddHours(-1),
                            Duration = null,
                            SyncStatus = SyncStatus.InSync
                        },
                    });

            protected override void AssertFinalState(
                AppServices services, ServerState finalServerState, DatabaseState finalDatabaseState)
            {
                finalDatabaseState.TimeEntries.Should().HaveCount(1);
            }
        }
    }
}
