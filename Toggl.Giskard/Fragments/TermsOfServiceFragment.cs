using System;
using System.Reactive.Disposables;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.Extensions;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Extensions.Reactive;

namespace Toggl.Giskard.Fragments
{
    [MvxDialogFragmentPresentation(AddToBackStack = true)]
    public sealed partial class TermsOfServiceFragment : MvxDialogFragment<TermsOfServiceViewModel>, IReactiveBindingHolder
    {
        public CompositeDisposable DisposeBag { get; } = new CompositeDisposable();

        public TermsOfServiceFragment() { }

        public TermsOfServiceFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.TermsOfServiceFragment, null);

            InitializeViews(view);
            bindViews();

            return view;
        }

        private void bindViews()
        {
            this.Bind(privacyPolicyTextView.Rx().Tap(), ViewModel.ViewPrivacyPolicy);
            this.Bind(termsOfServiceTextView.Rx().Tap(), ViewModel.ViewTermsOfService);
            this.Bind(acceptButton.Rx().Tap(), ViewModel.Close(true));
        }

        public override void OnResume()
        {
            base.OnResume();

            Dialog.Window.SetDefaultDialogLayout(Activity, Context, heightDp: 350);
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            ViewModel.Close(false).Execute();
        }
    }
}
