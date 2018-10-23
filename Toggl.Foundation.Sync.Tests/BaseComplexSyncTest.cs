using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Foundation.Sync.Tests.Helpers;
using Toggl.Foundation.Sync.Tests.State;
using Xunit;

namespace Toggl.Foundation.Sync.Tests
{
    public abstract class BaseComplexSyncTest
    {
        [Fact]
        public async Task Execute()
        {
            var server = await Server.Create();
            var storage = new Storage();
            var appServices = new AppServices(server.Api, storage.Database);

            // Arrange
            ArrangeServices(appServices);
            var definedServerState = ArrangeServerState(server.InitialServerState);
            var actualServerStateBefore = await server.Push(definedServerState);
            var definedDatabaseState = ArrangeDatabaseState(actualServerStateBefore);
            await storage.Store(definedDatabaseState);

            // Act
            await Act(appServices.SyncManager);

            // Assert
            AssertServices(appServices);
            var finalDatabaseState = await storage.LoadCurrentState();
            AssertFinalDatabaseState(finalDatabaseState);
            var finalServerState = await server.PullCurrentState();
            AssertFinalServerState(finalServerState);
        }

        protected virtual void ArrangeServices(AppServices services) { }
        protected abstract ServerState ArrangeServerState(ServerState initialServerState);
        protected abstract DatabaseState ArrangeDatabaseState(ServerState serverState);

        protected virtual async Task Act(ISyncManager syncManager)
        {
            await syncManager.ForceFullSync();
        }

        protected virtual void AssertServices(AppServices services) { }
        protected abstract void AssertFinalServerState(ServerState finalServerStateBefore);
        protected abstract void AssertFinalDatabaseState(DatabaseState finalDatabaseStateBefore);
    }
}
