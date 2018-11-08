using System.Net;
using System.Net.Http;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Tests.Integration.Helper
{
    internal static class TogglApiFactory
    {
        public static ITogglApi TogglApiWith(Credentials credentials)
        {
            var httpHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            var apiConfiguration = configurationFor(credentials);
            var apiClient = Ultrawave.TogglApiFactory.CreateDefaultApiClient(apiConfiguration.UserAgent, httpHandler);
            var retryingApiClient = new RetryingApiClient(apiClient);

            return new TogglApi(apiConfiguration, retryingApiClient);
        }

        private static ApiConfiguration configurationFor(Credentials credentials)
            => new ApiConfiguration(ApiEnvironment.Staging, credentials, Configuration.UserAgent);
    }
}
