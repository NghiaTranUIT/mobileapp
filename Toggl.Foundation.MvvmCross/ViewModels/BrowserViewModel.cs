using System;
using System.Reactive;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Foundation.MvvmCross.Parameters;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class BrowserViewModel : MvxViewModel<BrowserParameters>
    {
        private readonly IMvxNavigationService navigationService;

        public string Url { get; private set; }

        public string Title { get; private set; }

        public UIAction Close { get; }

        public BrowserViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            Close = UIAction.FromAsync(back);
        }

        public override void Prepare(BrowserParameters parameter)
        {
            Url = parameter.Url;
            Title = parameter.Title;
        }

        private Task back()
            => navigationService.Close(this);
    }
}
