using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions.Reactive;
using Toggl.Multivac.Extensions;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.StateVisible,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public partial class SendFeedbackActivity : ReactiveActivity<SendFeedbackViewModel>
    {
        private bool sendEnabled;
        private Subject<Unit> sendFeedbackSubject = new Subject<Unit>();
        private Subject<Unit> closeSubject = new Subject<Unit>();
        private Subject<string> errorTextSubject = new Subject<string>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SendFeedbackActivity);
            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_fade_out);

            InitializeViews();
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = GetString(Resource.String.SendFeedbackTitle);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            this.Bind(feedbackEditText.Rx().Text(), ViewModel.FeedbackText);
            this.Bind(errorCard.Rx().Tap(), ViewModel.DismissError);

            var sendButtonEnabled = ViewModel.SendEnabled.CombineLatest(ViewModel.IsLoading,
                (sendIsEnabled, isLoading) => sendIsEnabled && !isLoading);
           sendButtonEnabled
                .Subscribe(onSendEnabled)
                .DisposedBy(DisposeBag);

            ViewModel.Error
                .Select(selectErrorMessage)
                .Subscribe(errorTextSubject.OnNext)
                .DisposedBy(DisposeBag);
            this.Bind(errorTextSubject, errorInfoText.Rx().TextObserver());

            this.Bind(sendFeedbackSubject, ViewModel.Send);
            this.Bind(closeSubject, ViewModel.Close);
            this.Bind(ViewModel.Error.Select(error => error != null), errorCard.Rx().IsVisible());
            this.Bind(ViewModel.IsLoading, progressBar.Rx().IsVisible());
            this.Bind(ViewModel.IsLoading.Invert(), feedbackEditText.Rx().Enabled());
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.SendFeedbackMenu, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            var sendMenuItem = menu.FindItem(Resource.Id.SendMenuItem);
            sendMenuItem.SetEnabled(sendEnabled);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.SendMenuItem:
                    sendFeedbackSubject.OnNext(Unit.Default);
                    return true;

                case Android.Resource.Id.Home:
                    closeSubject.OnNext(Unit.Default);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnBackPressed()
        {
            closeSubject.OnNext(Unit.Default);
        }

        private void onSendEnabled(bool enabled)
        {
            sendEnabled = enabled;
            InvalidateOptionsMenu();
        }

        private string selectErrorMessage(Exception exception)
            => exception is OfflineException
                ? GetString(Resource.String.GenericInternetConnectionErrorMessage)
                : GetString(Resource.String.GenericErrorMessage);
    }
}
