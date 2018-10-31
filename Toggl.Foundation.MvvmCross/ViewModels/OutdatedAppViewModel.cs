using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class OutdatedAppViewModel : MvxViewModel
    {
        public UIAction OpenWebsite { get; }

        public UIAction UpdateApp { get; }

        private const string togglWebsiteUrl = "https://toggl.com";

        private readonly IBrowserService browserService;

        public OutdatedAppViewModel(IBrowserService browserService)
        {
            Ensure.Argument.IsNotNull(browserService, nameof(browserService));

            this.browserService = browserService;

            UpdateApp = UIAction.FromAction(updateApp);
            OpenWebsite = UIAction.FromAction(openWebsite);
        }

        private void openWebsite()
        {
            browserService.OpenUrl(togglWebsiteUrl);
        }

        private void updateApp()
        {
            browserService.OpenStore();
        }
    }
}
