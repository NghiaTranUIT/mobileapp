using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Models;

namespace Toggl.Ultrawave.Serialization
{
    public static class AppStateSerialization
    {
        public static string Serialize(
            IUser user,
            IPreferences preferences,
            IList<IWorkspace> workspaces,
            IList<ITimeEntry> timeEntries,
            IList<ITag> tags,
            IList<ITask> tasks,
            IList<IProject> projects,
            IList<IClient> clients,
            IList<IWorkspaceFeatureCollection> features,
            IDictionary<string, DateTimeOffset?> sinceDates)
        {
            var appState = new AppState
            {
                User = user as User ?? new User(user),
                Preferences = preferences as Preferences ?? new Preferences(preferences),
                Workspaces = workspaces?.Select(ws => ws as Workspace ?? new Workspace(ws)).ToList<IWorkspace>() ?? new List<IWorkspace>(),
                TimeEntries = timeEntries?.Select(te => te as TimeEntry ?? new TimeEntry(te)).ToList<ITimeEntry>() ?? new List<ITimeEntry>(),
                Tags = tags?.Select(tag => tag as Tag ?? new Tag(tag)).ToList<ITag>() ?? new List<ITag>(),
                Tasks = tasks?.Select(task => task as Task ?? new Task(task)).ToList<ITask>() ?? new List<ITask>(),
                Projects = projects?.Select(p => p as Project ?? new Project(p)).ToList<IProject>() ?? new List<IProject>(),
                Clients = clients?.Select(c => c as Client ?? new Client(c)).ToList<IClient>() ?? new List<IClient>(),
                Features = features?.Select(col => col as WorkspaceFeatureCollection
                    ?? new WorkspaceFeatureCollection(col)).ToList<IWorkspaceFeatureCollection>() ?? new List<IWorkspaceFeatureCollection>(),
                SinceDates = sinceDates ?? new Dictionary<string, DateTimeOffset?>(),
            };

            var serializer = new JsonSerializer();

            return serializer.Serialize(appState, SerializationReason.Post, appState.Features.FirstOrDefault());
        }

        private sealed class AppState
        {
            public IUser User { get; set; }
            public IPreferences Preferences { get; set; }
            public IList<IWorkspace> Workspaces { get; set; }
            public IList<ITimeEntry> TimeEntries { get; set; }
            public IList<ITag> Tags { get; set; }
            public IList<ITask> Tasks { get; set; }
            public IList<IProject> Projects { get; set; }
            public IList<IClient> Clients { get; set; }
            public IList<IWorkspaceFeatureCollection> Features { get; set; }
            public IDictionary<string, DateTimeOffset?> SinceDates { get; set; }
        }
    }
}
