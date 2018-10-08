using System.Reactive.Linq;
using CoreGraphics;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Calendar;

namespace Toggl.Daneel.ViewControllers.Calendar
{
    [ModalDialogPresentation]
    public sealed partial class SelectUserCalendarsViewController
        : ReactiveViewController<SelectUserCalendarsViewModel>
    {
        private const int heightAboveTableView = 98;
        private const int heightBelowTableView = 80;
        private const int maxHeight = 627;
        private const int width = 288;

        private const float enabledDoneButtonAlpha = 1;
        private const float disabledDoneButtonAlpha = 0.32f;

        public SelectUserCalendarsViewController() : base(null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new SelectUserCalendarsTableViewSource(TableView, ViewModel.Calendars);
            TableView.Source = source;

            this.Bind(DoneButton.Rx().Tap(), ViewModel.DoneAction);
            this.Bind(source.ItemSelected, ViewModel.SelectCalendarAction);
            this.Bind(ViewModel.DoneAction.Enabled, DoneButton.Rx().Enabled());
            this.Bind(ViewModel.DoneAction.Enabled.Select(alphaForEnabled), DoneButton.Rx().AnimatedAlpha());
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            setDialogSize();
        }

        private void setDialogSize()
        {
            var targetHeight = calculateTargetHeight();
            PreferredContentSize = new CGSize(
                width,
                targetHeight > maxHeight ? maxHeight : targetHeight
            );

            //Implementation in ModalPresentationController
            View.Frame = PresentationController.FrameOfPresentedViewInContainerView;

            TableView.ScrollEnabled = targetHeight > maxHeight;
        }

        private int calculateTargetHeight()
            => heightAboveTableView + heightBelowTableView + (int)TableView.ContentSize.Height;

        private float alphaForEnabled(bool enabled)
            => enabled ? enabledDoneButtonAlpha : disabledDoneButtonAlpha;
    }
}
