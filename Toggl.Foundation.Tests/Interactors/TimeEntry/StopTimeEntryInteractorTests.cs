using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace Toggl.Foundation.Tests.Interactors.TimeEntry
{
    public class StopTimeEntryInteractorTests : BaseInteractorTests
    {
        private const long ProjectId = 10;
        private const long WorkspaceId = 11;
        private const long UserId = 12;
        private const long CurrentRunningId = 13;
        private const long TaskId = 14;
        private static DateTimeOffset now = new DateTimeOffset(2018, 05, 14, 18, 00, 00, TimeSpan.Zero);

        private readonly StopTimeEntryInteractor interactor;

        private readonly TestScheduler testScheduler = new TestScheduler();

        private IThreadSafeTimeEntry TimeEntry { get; } =
            Models.TimeEntry.Builder
                .Create(CurrentRunningId)
                .SetUserId(UserId)
                .SetDescription("")
                .SetWorkspaceId(WorkspaceId)
                .SetSyncStatus(SyncStatus.InSync)
                .SetAt(now.AddDays(-1))
                .SetStart(now.AddHours(-2))
                .Build();

        private TimeEntryStopOrigin origin => TimeEntryStopOrigin.Deeplink;

        public StopTimeEntryInteractorTests()
        {
            var duration = (long)(now - TimeEntry.Start).TotalSeconds;
            var timeEntries = new List<IDatabaseTimeEntry>
                {
                    TimeEntry,
                    TimeEntry.With(duration),
                    TimeEntry.With(duration),
                    TimeEntry.With(duration)
                };

            DataSource
                .TimeEntries
                .GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Is(true))
                .Returns(callInfo =>
                    Observable
                         .Return(timeEntries)
                         .Select(x => x.Where(callInfo.Arg<Func<IDatabaseTimeEntry, bool>>()).Cast<IThreadSafeTimeEntry>()));

            interactor = new StopTimeEntryInteractor(TimeService, DataSource.TimeEntries, now, AnalyticsService, origin);
        }

        [Fact, LogIfTooSlow]
        public async ThreadingTask UpdatesTheTimeEntrySettingItsDuration()
        {
            await interactor.Execute();

            await DataSource.Received().TimeEntries.Update(Arg.Is<IThreadSafeTimeEntry>(te => te.Duration == (long)(now - te.Start).TotalSeconds));
        }

        [Fact, LogIfTooSlow]
        public async ThreadingTask UpdatesTheTimeEntryMakingItSyncNeeded()
        {
            await interactor.Execute();

            await DataSource.Received().TimeEntries.Update(Arg.Is<IThreadSafeTimeEntry>(te => te.SyncStatus == SyncStatus.SyncNeeded));
        }

        [Fact, LogIfTooSlow]
        public async ThreadingTask UpdatesTheInaccessibleRunningTimeEntry()
        {
            var workspace = new MockWorkspace(1, isInaccessible: true);

            var start = now.AddHours(-2);
            var duration = (long)(now - start).TotalSeconds;
            var timeEntries = new List<IDatabaseTimeEntry>
                {
                    new MockTimeEntry(33, workspace, start: start),
                    new MockTimeEntry(11, workspace, start: start, duration: duration),
                    new MockTimeEntry(22, workspace, start: start, duration: duration),
                };

            DataSource
                .TimeEntries
                .GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Is(true))
                .Returns(callInfo =>
                    Observable
                         .Return(timeEntries)
                         .Select(x => x.Where(callInfo.Arg<Func<IDatabaseTimeEntry, bool>>()).Cast<IThreadSafeTimeEntry>()));

            await interactor.Execute();

            await DataSource.Received().TimeEntries.Update(Arg.Is<IThreadSafeTimeEntry>(te => te.IsInaccessible == true));
        }

        [Fact, LogIfTooSlow]
        public async ThreadingTask SetsTheCurrentTimeAsAt()
        {
            await interactor.Execute();

            await DataSource.Received().TimeEntries.Update(Arg.Is<IThreadSafeTimeEntry>(te => te.At == now));
        }

        [Fact, LogIfTooSlow]
        public void ThrowsIfThereAreNoRunningTimeEntries()
        {
            long duration = (long)(now - TimeEntry.Start).TotalSeconds;
            var timeEntries = new List<IDatabaseTimeEntry>
                {
                    TimeEntry.With(duration),
                    TimeEntry.With(duration),
                    TimeEntry.With(duration)
                };

            DataSource
                .TimeEntries
                .GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Is(true))
                .Returns(callInfo =>
                    Observable
                         .Return(timeEntries)
                         .Select(x => x.Where(callInfo.Arg<Func<IDatabaseTimeEntry, bool>>()).Cast<IThreadSafeTimeEntry>()));

            var observer = testScheduler.CreateObserver<ITimeEntry>();
            var observable = interactor.Execute();
            observable.Subscribe(observer);

            observer.Messages.Single().Value.Exception.Should().BeOfType<NoRunningTimeEntryException>();
        }

        [Fact, LogIfTooSlow]
        public async ThreadingTask RegisterTrackingEvent()
        {
            await interactor.Execute();
            AnalyticsService.TimeEntryStopped.Received(1).Track(TimeEntryStopOrigin.Deeplink);
        }
    }
}
