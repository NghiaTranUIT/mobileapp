using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Navigation;
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
        private readonly IMvxNavigationService navigationService;

        public UIAction ViewTermsOfService { get; }

        public UIAction ViewPrivacyPolicy { get; }

        public UIAction Close { get; }

        public UIAction Accept { get; }

        public TermsOfServiceViewModel(
            IBrowserService browserService, IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(browserService, nameof(browserService));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;
            this.browserService = browserService;

            ViewPrivacyPolicy = UIAction.FromAction(() => openUrl(privacyPolicyUrl));
            ViewTermsOfService = UIAction.FromAction(() => openUrl(termsOfServiceUrl));

            Close = UIAction.FromAsync(() => close(false));
            Accept = UIAction.FromAsync(() => close(true));
        }

        private Task close(bool isAccepted)
            => navigationService.Close(this, isAccepted);

        private void openUrl(string url)
        {
            browserService.OpenUrl(url);
        }
    }
}
