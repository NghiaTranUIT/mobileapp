using System.Collections.Generic;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests
{
    public struct ServerState
    {
        public IUser User { get; }
        public IList<IClient> Clients { get; }
        public IList<IProject> Projects { get; }
        public IPreferences Preferences { get; }
        public IList<ITag> Tags { get; }
        public IList<ITask> Tasks { get; }
        public IList<ITimeEntry> TimeEntries { get; }
        public IList<IWorkspace> Workspaces { get; }

        public ServerState(
            IUser user,
            IList<IClient> clients,
            IList<IProject> projects,
            IPreferences preferences,
            IList<ITag> tags,
            IList<ITask> tasks,
            IList<ITimeEntry> timeEntries,
            IList<IWorkspace> workspaces)
        {
            User = user;
            Clients = clients;
            Projects = projects;
            Preferences = preferences;
            Tags = tags;
            Tasks = tasks;
            TimeEntries = timeEntries;
            Workspaces = workspaces;
        }
    }
}
