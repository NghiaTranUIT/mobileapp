using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Xunit;
using System.Reactive.Linq;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class TermsOfServiceViewModelTests
    {
        public abstract class TermsOfServiceViewModelTest
            : BaseViewModelTests<TermsOfServiceViewModel>
        {
            protected override TermsOfServiceViewModel CreateViewModel()
                => new TermsOfServiceViewModel(BrowserService, NavigationService);
        }

        public sealed class TheConstructor : TermsOfServiceViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useBrowserService,
                bool useNavigationService)
            {
                var browserService = useBrowserService ? BrowserService : null;
                var navigationService = useNavigationService ? NavigationService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new TermsOfServiceViewModel(browserService, navigationService);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheViewTermsOfServiceCommand : TermsOfServiceViewModelTest
        {
            private const string termsOfServiceUrl = "https://toggl.com/legal/terms/";

            [Fact, LogIfTooSlow]
            public async void OpensTermsOfService()
            {
                await ViewModel.ViewTermsOfService.Execute();

                BrowserService.Received().OpenUrl(termsOfServiceUrl);
            }
        }

        public sealed class TheViewPrivacyPolicyCommand : TermsOfServiceViewModelTest
        {
            private const string privacyPolicyUrl = "https://toggl.com/legal/privacy/";

            [Fact, LogIfTooSlow]
            public async void OpensPrivacyPolicy()
            {
                await ViewModel.ViewPrivacyPolicy.Execute();

                BrowserService.Received().OpenUrl(privacyPolicyUrl);
            }
        }

        public sealed class TheCloseCommand : TermsOfServiceViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModelAndReturnsFalse()
            {
                await ViewModel.Close.Execute();

                await NavigationService.Received().Close(ViewModel, false);
            }
        }

        public sealed class TheAcceptCommand : TermsOfServiceViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModelAndReturnsTrue()
            {
                await ViewModel.Accept.Execute();

                await NavigationService.Received().Close(ViewModel, true);
            }
        }
    }
}
