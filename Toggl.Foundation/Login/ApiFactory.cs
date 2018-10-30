using System.Net;
using Toggl.Multivac;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Login
{
    public sealed class ApiFactory : IApiFactory
    {
        public UserAgent UserAgent { get; }

        public ApiEnvironment Environment { get; }

        public IWebProxy Proxy { get; }

        public ApiFactory(ApiEnvironment apiEnvironment, UserAgent userAgent, IWebProxy proxy)
        {
            Ensure.Argument.IsNotNull(userAgent, nameof(userAgent));
            Ensure.Argument.IsNotNull(proxy, nameof(proxy));

            UserAgent = userAgent;
            Environment = apiEnvironment;
            Proxy = proxy;
        }

        public ITogglApi CreateApiWith(Credentials credentials)
        {
            var configuration = new ApiConfiguration(Environment, credentials, UserAgent);
            return TogglApiFactory.WithConfiguration(configuration, Proxy);
        }
    }
}
