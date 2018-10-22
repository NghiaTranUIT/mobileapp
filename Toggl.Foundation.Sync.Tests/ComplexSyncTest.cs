using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Realms.Sync;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Realm;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Xunit;
using Credentials = Toggl.Ultrawave.Network.Credentials;
using TogglApiFactory = Toggl.Ultrawave.Tests.Integration.Helper.TogglApiFactory;

namespace Toggl.Foundation.Sync.Tests
{
    public abstract class ComplexSyncTest
    {
        protected IScheduler Scheduler { get; }

        protected ITimeService TimeService { get; }

        protected IErrorHandlingService ErrorHandlingService { get; } = Substitute.For<IErrorHandlingService>();

        protected IBackgroundService BackgroundService { get; } = Substitute.For<IBackgroundService>();

        protected IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();

        protected ILastTimeUsageStorage LastTimeUsageStorage { get; } = Substitute.For<ILastTimeUsageStorage>();

        protected INotificationService NotificationService { get; } = Substitute.For<INotificationService>();

        protected IApplicationShortcutCreator ApplicationShortcutCreator { get; } =
            Substitute.For<IApplicationShortcutCreator>();

        private readonly TimeSpan retryLimit = TimeSpan.FromSeconds(60);

        private readonly TimeSpan minimumTimeInBackgroundForFullSync = TimeSpan.FromMinutes(5);

        protected ComplexSyncTest()
        {
            Scheduler = System.Reactive.Concurrency.Scheduler.Default;
            TimeService = new TimeService(Scheduler);
        }

        [Fact]
        public async Task Execute()
        {
            var server = new Server();
            await server.Initialize();

            var storage = new Storage();

            var definedServerState = PrepareServerState(server.User);
            await server.Push(definedServerState);
            var actualServerStateBefore = await server.Pull();
            var definedDatabaseState = PrepareDatabaseState(actualServerStateBefore);
            storage.Store(definedDatabaseState);

            var syncManager = initializeDataSource(server.Api, storage.Database);
            await Act(syncManager);

            var expectedServerState = ExpectedServerState(actualServerStateBefore);
            var expectedDatabaseState = ExpectedDatabaseState(definedDatabaseState);

            var actualDatabaseStateAfter = storage.Load();
            var actualServerStateAfter = await server.Pull();

            actualDatabaseStateAfter.Should().BeEquivalentTo(expectedDatabaseState, options => options.IncludingProperties());
            actualServerStateAfter.Should().BeEquivalentTo(expectedServerState, options => options.IncludingProperties());
        }

        protected abstract ServerState PrepareServerState(IUser user);
        protected abstract DatabaseState PrepareDatabaseState(ServerState serverState);

        protected virtual async Task Act(ISyncManager syncManager)
        {
            await syncManager.ForceFullSync();
        }

        protected abstract ServerState ExpectedServerState(ServerState serverStateBefore);
        protected abstract DatabaseState ExpectedDatabaseState(DatabaseState databaseStateBefore);

        private ISyncManager initializeDataSource(ITogglApi api, ITogglDatabase database)
        {
            ISyncManager createSyncManager(ITogglDataSource dataSource)
            {
                return TogglSyncManager.CreateSyncManager(
                    database,
                    api,
                    dataSource,
                    TimeService,
                    AnalyticsService,
                    LastTimeUsageStorage,
                    TimeSpan.FromSeconds(60),
                    Scheduler);
            }

            var togglDataSource = new TogglDataSource(
                api,
                database,
                TimeService,
                ErrorHandlingService,
                BackgroundService,
                createSyncManager,
                minimumTimeInBackgroundForFullSync,
                NotificationService,
                ApplicationShortcutCreator,
                AnalyticsService);

            return togglDataSource.SyncManager;
        }
    }
}
