using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Multivac.Models;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Sync.Tests
{
    public sealed class Server
    {
        public ITogglApi Api { get; private set; }

        public IUser User { get; private set; }

        public async Task Initialize()
        {
            User = await Ultrawave.Tests.Integration.User.Create();
            var credentials = Credentials.WithApiToken(User.ApiToken);
            Api = Ultrawave.Tests.Integration.Helper.TogglApiFactory.TogglApiWith(credentials);
        }

        public async Task<ServerState> Pull()
        {
            var user = await Api.User.Get();
            var clients = await Api.Clients.GetAll();
            var projects = await Api.Projects.GetAll();
            var preferences = await Api.Preferences.Get();
            var tags = await Api.Tags.GetAll();
            var tasks = await Api.Tasks.GetAll();
            var timeEntries = await Api.TimeEntries.GetAll();
            var workspaces = await Api.Workspaces.GetAll();

            return new ServerState(
                user, clients, projects, preferences, tags, tasks, timeEntries, workspaces);
        }

        public async Task Push(ServerState state)
        {
        }
    }
}
