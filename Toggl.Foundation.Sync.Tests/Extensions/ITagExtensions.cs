using System;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests.Extensions
{
    public static class ITagExtensions
    {
        private sealed class Tag : ITag
        {
            public long Id { get; set; }
            public DateTimeOffset? ServerDeletedAt { get; set; }
            public DateTimeOffset At { get; set; }
            public long WorkspaceId { get; set; }
            public string Name { get; set; }
        }

        public static ITag With(
            this ITag tag,
            New<long> workspaceId = default(New<long>))
            => new Tag
            {
                Id = tag.Id,
                ServerDeletedAt = tag.ServerDeletedAt,
                At = tag.At,
                Name = tag.Name,
                WorkspaceId = workspaceId.ValueOr(tag.WorkspaceId)
            };
    }
}
