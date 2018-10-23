using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class OutdatedAppViewModelTests
    {
        public abstract class OutdatedAppViewModelTest : BaseViewModelTests<OutdatedAppViewModel>
        {
            protected override OutdatedAppViewModel CreateViewModel()
                => new OutdatedAppViewModel(BrowserService);
        }

        public sealed class TheConstructor : OutdatedAppViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfTheArgumentIsNull()
            {
                Action tryingToConstructWithEmptyParameters =
                    () => new OutdatedAppViewModel(null);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheUpdateAppAction : OutdatedAppViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task CallsTheOpenStoreMethodOfTheBrowserService()
            {
                await ViewModel.UpdateAppAction.Execute();

                BrowserService.Received().OpenStore();
            }
        }

        public sealed class TheOpenWebsiteCommand : OutdatedAppViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task CallsTheOpenWebsiteMethodOfTheBrowserService()
            {
                const string togglWebsiteUrl = "https://toggl.com";

                await ViewModel.OpenWebsiteAction.Execute();

                BrowserService.Received().OpenUrl(Arg.Is(togglWebsiteUrl));
            }
        }
    }
}
