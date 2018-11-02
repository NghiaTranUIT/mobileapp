using MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class TermsOfServiceViewModel : MvxViewModelResult<bool>
    {
        private const string privacyPolicyUrl = "https://toggl.com/legal/privacy/";
        private const string termsOfServiceUrl = "https://toggl.com/legal/terms/";

        private readonly IBrowserService browserService;

        public UIAction ViewTermsOfService { get; }

        public UIAction ViewPrivacyPolicy { get; }

        public TermsOfServiceViewModel(IBrowserService browserService)
        {
            Ensure.Argument.IsNotNull(browserService, nameof(browserService));

            this.browserService = browserService;

            ViewPrivacyPolicy = UIAction.FromAction(() => openUrl(privacyPolicyUrl));
            ViewTermsOfService = UIAction.FromAction(() => openUrl(termsOfServiceUrl));
        }

        private void openUrl(string url)
        {
            browserService.OpenUrl(url);
        }
    }
}
