using MvvmCross.Platforms.Ios.Presenters.Attributes;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;

namespace Toggl.Daneel.ViewControllers
{
    [MvxRootPresentation]
    public sealed partial class OutdatedAppViewController : ReactiveViewController<OutdatedAppViewModel>
    {
        public OutdatedAppViewController()
            : base(nameof(OutdatedAppViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UpdateButton.Rx()
                .BindAction(ViewModel.UpdateApp)
                .DisposedBy(DisposeBag);

            WebsiteButton.Rx()
                .BindAction(ViewModel.OpenWebsite)
                .DisposedBy(DisposeBag);
        }
    }
}
