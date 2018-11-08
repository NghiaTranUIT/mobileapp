using System.Net.Http;
using Toggl.Multivac;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Login
{
    public sealed class ApiFactory : IApiFactory
    {
        public UserAgent UserAgent { get; }

        public ApiEnvironment Environment { get; }

        private readonly HttpMessageHandler httpHandler;

        public ApiFactory(ApiEnvironment apiEnvironment, UserAgent userAgent, HttpMessageHandler httpHandler)
        {
            Ensure.Argument.IsNotNull(userAgent, nameof(userAgent));
            Ensure.Argument.IsNotNull(httpHandler, nameof(httpHandler));

            UserAgent = userAgent;
            Environment = apiEnvironment;
            this.httpHandler = httpHandler;
        }

        public ITogglApi CreateApiWith(Credentials credentials)
        {
            var configuration = new ApiConfiguration(Environment, credentials, UserAgent);
            return TogglApiFactory.WithConfiguration(configuration, httpHandler);
        }
    }
}
