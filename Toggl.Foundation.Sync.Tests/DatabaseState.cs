using System;
using System.Collections;
using System.Collections.Generic;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.Tests
{
    public struct DatabaseState
    {
        public IThreadSafeUser User { get; }
        public ISet<IThreadSafeClient> Clients { get; }
        public ISet<IThreadSafeProject> Projects { get; }
        public IThreadSafePreferences Preferences { get; }
        public ISet<IThreadSafeTag> Tags { get; }
        public ISet<IThreadSafeTask> Tasks { get; }
        public ISet<IThreadSafeTimeEntry> TimeEntries { get; }
        public ISet<IThreadSafeWorkspace> Workspaces { get; }
        public ISet<IThreadSafeWorkspaceFeatureCollection> WorkspaceFeatures { get; }
        public IDictionary<Type, DateTimeOffset> SinceParameters { get; }

        public DatabaseState(
            IThreadSafeUser user,
            IEnumerable<IThreadSafeClient> clients,
            IEnumerable<IThreadSafeProject> projects,
            IThreadSafePreferences preferences,
            IEnumerable<IThreadSafeTag> tags,
            IEnumerable<IThreadSafeTask> tasks,
            IEnumerable<IThreadSafeTimeEntry> timeEntries,
            IEnumerable<IThreadSafeWorkspace> workspaces,
            IEnumerable<IThreadSafeWorkspaceFeatureCollection> workspaceFeatures,
            IDictionary<Type, DateTimeOffset> sinceParameters)
        {
            User = user;
            Clients = new HashSet<IThreadSafeClient>(clients);
            Projects = new HashSet<IThreadSafeProject>(projects);
            Preferences = preferences;
            Tags = new HashSet<IThreadSafeTag>(tags);
            Tasks = new HashSet<IThreadSafeTask>(tasks);
            TimeEntries = new HashSet<IThreadSafeTimeEntry>(timeEntries);
            Workspaces = new HashSet<IThreadSafeWorkspace>(workspaces);
            WorkspaceFeatures = new HashSet<IThreadSafeWorkspaceFeatureCollection>(workspaceFeatures);
            SinceParameters = sinceParameters;
        }
    }
}
