using System;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Calendar;
using Toggl.Foundation.Helper;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant;
using Xunit;

namespace Toggl.Foundation.Tests
{
    public sealed class CalendarItemTests
    {
        public sealed class TheFromTimeEntryMethod
        {
            [Theory, LogIfTooSlow]
            [InlineData(SyncStatus.InSync, CalendarIconKind.None)]
            [InlineData(SyncStatus.RefetchingNeeded, CalendarIconKind.None)]
            [InlineData(SyncStatus.SyncFailed, CalendarIconKind.Unsyncable)]
            [InlineData(SyncStatus.SyncNeeded, CalendarIconKind.Unsynced)]
            public void SetsTheAppropriateCalendarIconKind(SyncStatus syncStatus, CalendarIconKind expectedCalendarIcon)
            {
                var timeEntry = new MockTimeEntry { SyncStatus = syncStatus };

                var calendarItem = CalendarItem.From(timeEntry);

                calendarItem.IconKind.Should().Be(expectedCalendarIcon);
            }
        }

        public sealed class TheDescriptionProperty
        {
            [Theory, LogIfTooSlow]
            [InlineData(null)]
            [InlineData("")]
            public void ShouldUseTheDefaultValueIfNullOrEmpty(string description)
            {
                var startTime = new DateTimeOffset(2018, 8, 22, 12, 0, 0, TimeSpan.Zero);
                var duration = TimeSpan.FromMinutes(30);

                var calendarItem = new CalendarItem("id", CalendarItemSource.Calendar, startTime, duration, description, CalendarIconKind.None);
                calendarItem.Description.Should().Be(Resources.NoDescription);
            }
        }

        public sealed class TheColorProperty
        {
            [Fact, LogIfTooSlow]
            public void ShouldHaveADefaultValue()
            {
                var timeEntry = new MockTimeEntry();

                var calendarItem = CalendarItem.From(timeEntry);

                calendarItem.Color.Should().Be(Color.NoProject);
            }

            [Fact, LogIfTooSlow]
            public void ShouldInheritFromTimeEntryProjectColor()
            {
                var timeEntry = new MockTimeEntry { Project = new MockProject { Color = "#666666" } };

                var calendarItem = CalendarItem.From(timeEntry);

                calendarItem.Color.Should().Be("#666666");
            }

            [Fact, LogIfTooSlow]
            public void ShouldUseDefaultValueIfPassedAnInvalidColor()
            {
                var timeEntry = new MockTimeEntry { Project = new MockProject { Color = "#fa" } };

                var calendarItem = CalendarItem.From(timeEntry);

                calendarItem.Color.Should().Be(Color.NoProject);
            }
        }

        public sealed class TheDurationProperty
        {
            [Property]
            public void ShouldBeTakenFromTimeEntry(long duration)
            {
                var timeEntry = new MockTimeEntry { Duration = duration };

                var calendarItem = CalendarItem.From(timeEntry);

                calendarItem.Duration.Should().Be(TimeSpan.FromSeconds(duration));
            }
        }
    }
}
