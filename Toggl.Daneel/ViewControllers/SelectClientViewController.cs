using System.Reactive;
using System.Threading.Tasks;
using MvvmCross.Binding.BindingContext;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
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

            var source = new ClientTableViewSource(SuggestionsTableView, ViewModel.Clients);
            SuggestionsTableView.Source = source;

            this.Bind(CloseButton.Rx().Tap(), ViewModel.CloseAction);

            SearchTextField.BecomeFirstResponder();
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
