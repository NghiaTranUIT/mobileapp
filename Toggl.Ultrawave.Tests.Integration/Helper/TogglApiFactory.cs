using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Tests.Integration.Helper
{
    public static class TogglApiFactory
    {
        public static ITogglApi TogglApiWith(Credentials credentials)
        {
            var apiConfiguration = configurationFor(credentials);
            var apiClient = Ultrawave.TogglApiFactory.CreateDefaultApiClient(apiConfiguration.UserAgent);
            var retryingApiClient = new RetryingApiClient(apiClient);

            return new TogglApi(apiConfiguration, retryingApiClient);
        }

        private static ApiConfiguration configurationFor(Credentials credentials)
            => new ApiConfiguration(ApiEnvironment.Staging, credentials, Configuration.UserAgent);
    }
}
