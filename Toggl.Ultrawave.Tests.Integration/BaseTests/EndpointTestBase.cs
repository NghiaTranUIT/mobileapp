using System.Threading.Tasks;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Tests.Integration.BaseTests
{
    public abstract class EndpointTestBase
    {
        protected async Task<(ITogglApi togglClient, IUser user)> SetupTestUser()
        {
            var user = await User.Create();
            var credentials = Credentials.WithApiToken(user.ApiToken);
            var togglApi = TogglApiWith(credentials);

            return (togglApi, user);
        }

        protected ITogglApi TogglApiWith(Credentials credentials)
            => Helper.TogglApiFactory.TogglApiWith(credentials);
    }
}
