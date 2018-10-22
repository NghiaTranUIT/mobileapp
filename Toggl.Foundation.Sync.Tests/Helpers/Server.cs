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
        }

        public async Task Push(ServerState state)
        {
        }
    }
}
