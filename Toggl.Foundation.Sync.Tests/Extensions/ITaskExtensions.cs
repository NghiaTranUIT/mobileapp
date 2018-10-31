using System;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests.Extensions
{
    public static class ITaskExtensions
    {
        private sealed class Task : ITask
        {
            public long Id { get; set; }
            public DateTimeOffset At { get; set; }
            public string Name { get; set; }
            public long ProjectId { get; set; }
            public long WorkspaceId { get; set; }
            public long? UserId { get; set; }
            public long EstimatedSeconds { get; set; }
            public bool Active { get; set; }
            public long TrackedSeconds { get; set; }
        }

        public static ITask With(
            this ITask task,
            New<long> workspaceId = default(New<long>),
            New<long> projectId = default(New<long>),
            New<long?> userId = default(New<long?>))
            => new Task
            {
                Id = task.Id,
                At = task.At,
                Name = task.Name,
                ProjectId = projectId.ValueOr(task.ProjectId),
                WorkspaceId = workspaceId.ValueOr(task.WorkspaceId),
                UserId = userId.ValueOr(task.UserId),
                EstimatedSeconds = task.EstimatedSeconds,
                Active = task.Active,
                TrackedSeconds = task.TrackedSeconds
            };
    }
}
