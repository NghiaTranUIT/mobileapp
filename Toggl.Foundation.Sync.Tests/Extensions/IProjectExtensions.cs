using System;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests.Extensions
{
    public static class IProjectExtensions
    {
        private sealed class Project : IProject
        {
            public long Id { get; set; }
            public DateTimeOffset? ServerDeletedAt { get; set; }
            public DateTimeOffset At { get; set; }
            public long WorkspaceId { get; set; }
            public long? ClientId { get; set; }
            public string Name { get; set; }
            public bool IsPrivate { get; set; }
            public bool Active { get; set; }
            public string Color { get; set; }
            public bool? Billable { get; set; }
            public bool? Template { get; set; }
            public bool? AutoEstimates { get; set; }
            public long? EstimatedHours { get; set; }
            public double? Rate { get; set; }
            public string Currency { get; set; }
            public int? ActualHours { get; set; }
        }

        public static IProject With(
            this IProject project,
            New<long> workspaceId = default(New<long>),
            New<long?> clientId = default(New<long?>))
            => new Project
            {
                Id = project.Id,
                ServerDeletedAt = project.ServerDeletedAt,
                At = project.At,
                WorkspaceId = workspaceId.ValueOr(project.WorkspaceId),
                ClientId = clientId.ValueOr(project.ClientId),
                Name = project.Name,
                IsPrivate = project.IsPrivate,
                Active = project.Active,
                Color = project.Color,
                Billable = project.Billable,
                Template = project.Template,
                AutoEstimates = project.AutoEstimates,
                EstimatedHours = project.EstimatedHours,
                Rate = project.Rate,
                Currency = project.Currency,
                ActualHours = project.ActualHours
            };
    }
}
