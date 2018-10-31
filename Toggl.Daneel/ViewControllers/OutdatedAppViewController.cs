using System;
using CoreGraphics;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Views;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [ModalDialogPresentation]
    public sealed partial class OutdatedAppViewController : ReactiveViewController<OutdatedAppViewModel>
    {
        private const int cardHeight = 357;

        public OutdatedAppViewController()
            : base(nameof(OutdatedAppViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var screenWidth = UIScreen.MainScreen.Bounds.Width;
            PreferredContentSize = new CGSize
            {
                // ScreenWidth - 32 for 16pt margins on both sides
                Width = screenWidth > 320 ? screenWidth - 32 : 312,
                Height = cardHeight
            };

            this.Bind(UpdateButton.Rx().Tap(), ViewModel.UpdateApp);
            this.Bind(WebsiteButton.Rx().Tap(), ViewModel.OpenWebsite);
        }

    }
}
