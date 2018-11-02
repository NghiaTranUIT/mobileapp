using System;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests.Extensions
{
    public static class ITimeEntryExtensions
    {
        private sealed class TimeEntry : ITimeEntry
        {
            public long Id { get; set; }
            public DateTimeOffset? ServerDeletedAt { get; set; }
            public DateTimeOffset At { get; set; }
            public long WorkspaceId { get; set; }
            public long? ProjectId { get; set; }
            public long? TaskId { get; set; }
            public bool Billable { get; set; }
            public DateTimeOffset Start { get; set; }
            public long? Duration { get; set; }
            public string Description { get; set; }
            public IEnumerable<long> TagIds { get; set; }
            public long UserId { get; set; }
        }

        public static ITimeEntry With(
            this ITimeEntry timeEntry,
            New<long> workspaceId = default(New<long>),
            New<IEnumerable<long>> tagIds = default(New<IEnumerable<long>>),
            New<long?> taskId = default(New<long?>),
            New<long?> projectId = default(New<long?>))
            => new TimeEntry
            {
                Id = timeEntry.Id,
                ServerDeletedAt = timeEntry.ServerDeletedAt,
                At = timeEntry.At,
                WorkspaceId = workspaceId.ValueOr(timeEntry.WorkspaceId),
                ProjectId = projectId.ValueOr(timeEntry.ProjectId),
                TaskId = taskId.ValueOr(timeEntry.TaskId),
                Billable = timeEntry.Billable,
                Start = timeEntry.Start,
                Duration = timeEntry.Duration,
                Description = timeEntry.Description,
                TagIds = tagIds.ValueOr(timeEntry.TagIds),
                UserId = timeEntry.UserId
            };
    }
}
