using System;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests.Extensions
{
    public static class IClientExtensions
    {
        private sealed class Client : IClient
        {
            public long Id { get; set; }
            public DateTimeOffset? ServerDeletedAt { get; set; }
            public DateTimeOffset At { get; set; }
            public long WorkspaceId { get; set; }
            public string Name { get; set; }
        }

        public static IClient With(
            this IClient client,
            New<long> workspaceId = default(New<long>))
            => new Client
            {
                Id = client.Id,
                ServerDeletedAt = client.ServerDeletedAt,
                At = client.At,
                Name = client.Name,
                WorkspaceId = workspaceId.ValueOr(client.WorkspaceId)
            };
    }
}
