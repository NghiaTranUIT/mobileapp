using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Binding.BindingContext;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public partial class SelectClientViewController : KeyboardAwareViewController<SelectClientViewModel>, IDismissableViewController
    {
        public SelectClientViewController()
            : base(nameof(SelectClientViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableViewSource = new ClientTableViewSource(SuggestionsTableView, ViewModel.Clients);

            this.Bind(CloseButton.Rx().Tap(), ViewModel.CloseAction);
            this.Bind(SearchTextField.Rx().Text(), ViewModel.ClientFilterText);
            this.Bind(tableViewSource.ItemSelected, ViewModel.SelectClientAction);
            this.Bind(tableViewSource.SuggestCreationTapped, ViewModel.CreateClientAction);

            this.Bind(ViewModel.CreationSuggestion, tableViewSource.SuggestCreation);
            this.Bind(ViewModel.ClientFilterText, tableViewSource.SuggestCreationName);

            SuggestionsTableView
                .Rx()
                .Bind(tableViewSource)
                .DisposedBy(DisposeBag);

            SearchTextField.BecomeFirstResponder();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            ViewModel.DisposeBag.Dispose();
        }

        public async Task<bool> Dismiss()
        {
            ViewModel.CloseAction.Execute(Unit.Default);
            return true;
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            BottomConstraint.Constant = e.FrameEnd.Height;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            BottomConstraint.Constant = 0;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }
    }
}
