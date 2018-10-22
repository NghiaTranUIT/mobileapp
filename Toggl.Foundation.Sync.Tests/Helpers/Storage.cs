using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Realm;

namespace Toggl.Foundation.Sync.Tests
{
    public sealed class Storage
    {
        public ITogglDatabase Database { get; }

        public Storage()
        {
            Database = new Database();
        }

        public async Task<DatabaseState> Load()
        {
            var user = Models.User.From(await Database.User.Single());
            var clients = await Database.Clients.GetAll().Select(c => c.Select(Models.Client.From));
            var projects = await Database.Projects.GetAll().Select(p => p.Select(Models.Project.From));
            var preferences = Models.Preferences.From(await Database.Preferences.Single());
            var tags = await Database.Tags.GetAll().Select(t => t.Select(Models.Tag.From));
            var tasks = await Database.Tasks.GetAll().Select(t => t.Select(Models.Task.From));
            var timeEntries = await Database.TimeEntries.GetAll().Select(te => te.Select(Models.TimeEntry.From));
            var workspaces = await Database.Workspaces.GetAll().Select(ws => ws.Select(Models.Workspace.From));
            var sinceParameters = new Dictionary<Type, DateTimeOffset?>
            {
                [typeof(IDatabaseClient)] = Database.SinceParameters.Get<IDatabaseClient>(),
                [typeof(IDatabaseProject)] = Database.SinceParameters.Get<IDatabaseProject>(),
                [typeof(IDatabaseTag)] = Database.SinceParameters.Get<IDatabaseTag>(),
                [typeof(IDatabaseTask)] = Database.SinceParameters.Get<IDatabaseTask>(),
                [typeof(IDatabaseTimeEntry)] = Database.SinceParameters.Get<IDatabaseTimeEntry>(),
                [typeof(IDatabaseWorkspace)] = Database.SinceParameters.Get<IDatabaseWorkspace>()
            };

            return new DatabaseState(
                user, clients, projects, preferences, tags, tasks, timeEntries, workspaces, sinceParameters);
        }

        public void Store(DatabaseState databaseState)
        {
        }
    }
}
