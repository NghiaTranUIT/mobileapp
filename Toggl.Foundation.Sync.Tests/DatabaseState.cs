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
        public IList<IThreadSafeClient> Clients { get; }
        public IList<IThreadSafeProject> Projects { get; }
        public IThreadSafePreferences Preferences { get; }
        public IList<IThreadSafeTag> Tags { get; }
        public IList<IThreadSafeTask> Tasks { get; }
        public IList<IThreadSafeTimeEntry> TimeEntries { get; }
        public IList<IThreadSafeWorkspace> Workspaces { get; }
        public IList<IThreadSafeWorkspaceFeatureCollection> WorkspaceFeatures { get; }
        public IDictionary<Type, DateTimeOffset> SinceParameters { get; }

        public DatabaseState(
            IThreadSafeUser user,
            IList<IThreadSafeClient> clients,
            IList<IThreadSafeProject> projects,
            IThreadSafePreferences preferences,
            IList<IThreadSafeTag> tags,
            IList<IThreadSafeTask> tasks,
            IList<IThreadSafeTimeEntry> timeEntries,
            IList<IThreadSafeWorkspace> workspaces,
            IList<IThreadSafeWorkspaceFeatureCollection> workspaceFeatures,
            IDictionary<Type, DateTimeOffset> sinceParameters)
        {
            User = user;
            Clients = clients;
            Projects = projects;
            Preferences = preferences;
            Tags = tags;
            Tasks = tasks;
            TimeEntries = timeEntries;
            Workspaces = workspaces;
            WorkspaceFeatures = workspaceFeatures;
            SinceParameters = sinceParameters;
        }
    }
}
