using System.Collections.Generic;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests.State
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
            IEnumerable<IClient> clients = null,
            IEnumerable<IProject> projects = null,
            IPreferences preferences = null,
            IEnumerable<ITag> tags = null,
            IEnumerable<ITask> tasks = null,
            IEnumerable<ITimeEntry> timeEntries = null,
            IEnumerable<IWorkspace> workspaces = null)
        {
            User = user;
            Clients = new HashSet<IClient>(clients ?? new IClient[0]);
            Projects = new HashSet<IProject>(projects ?? new IProject[0]);
            Preferences = preferences ?? new MockPreferences();
            Tags = new HashSet<ITag>(tags ?? new ITag[0]);
            Tasks = new HashSet<ITask>(tasks ?? new ITask[0]);
            TimeEntries = new HashSet<ITimeEntry>(timeEntries ?? new ITimeEntry[0]);
            Workspaces = new HashSet<IWorkspace>(workspaces ?? new IWorkspace[0]);
        }
    }
}
