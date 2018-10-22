using System.Collections.Generic;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests
{
    public struct ServerState
    {
        public IUser User { get; }
        public ISet<IClient> Clients { get; }
        public ISet<IProject> Projects { get; }
        public IPreferences Preferences { get; }
        public ISet<ITag> Tags { get; }
        public ISet<ITask> Tasks { get; }
        public ISet<ITimeEntry> TimeEntries { get; }
        public ISet<IWorkspace> Workspaces { get; }

        public ServerState(
            IUser user,
            IEnumerable<IClient> clients,
            IEnumerable<IProject> projects,
            IPreferences preferences,
            IEnumerable<ITag> tags,
            IEnumerable<ITask> tasks,
            IEnumerable<ITimeEntry> timeEntries,
            IEnumerable<IWorkspace> workspaces)
        {
            User = user;
            Clients = new HashSet<IClient>(clients);
            Projects = new HashSet<IProject>(projects);
            Preferences = preferences;
            Tags = new HashSet<ITag>(tags);
            Tasks = new HashSet<ITask>(tasks);
            TimeEntries = new HashSet<ITimeEntry>(timeEntries);
            Workspaces = new HashSet<IWorkspace>(workspaces);
        }
    }
}
