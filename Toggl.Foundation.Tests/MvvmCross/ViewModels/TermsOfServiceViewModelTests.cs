using System;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class TermsOfServiceViewModelTests
    {
        public abstract class TermsOfServiceViewModelTest
            : BaseViewModelTests<TermsOfServiceViewModel>
        {
            protected override TermsOfServiceViewModel CreateViewModel()
                => new TermsOfServiceViewModel(BrowserService);
        }

        public sealed class TheConstructor : TermsOfServiceViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useBrowserService)
            {
                var browserService = useBrowserService ? BrowserService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new TermsOfServiceViewModel(browserService);

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
    }
}
