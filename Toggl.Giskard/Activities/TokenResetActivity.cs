using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Extensions.Reactive;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Resources;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class TokenResetActivity : ReactiveActivity<TokenResetViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.TokenResetActivity);
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
        }

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);

            toolbar.Title = LoginTitle;
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetDisplayShowHomeEnabled(false);
            this.CancelAllNotifications();

            this.Bind(ViewModel.Email.SelectToString(), emailLabel.Rx().TextObserver());
            this.Bind(ViewModel.Password.SelectToString(), passwordEditText.Rx().TextObserver());

            this.Bind(ViewModel.IsLoading, progressBar.Rx().IsVisible());

            this.Bind(signoutLabel.Rx().Tap(), ViewModel.SignOut);
            this.Bind(doneButton.Rx().Tap(), ViewModel.Done);
        }
    }
}