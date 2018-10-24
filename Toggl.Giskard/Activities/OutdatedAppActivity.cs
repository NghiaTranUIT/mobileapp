using System.Reactive.Disposables;
using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions.Reactive;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme.OutdatedAppStatusBarColor",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class OutdatedAppActivity : MvxAppCompatActivity<OutdatedAppViewModel>, IReactiveBindingHolder
    {
        public CompositeDisposable DisposeBag { get; } = new CompositeDisposable();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.OutdatedAppActivity);
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
            initializeViews();

            this.Bind(updateAppButton.Rx().Tap(), ViewModel.UpdateAppAction);
            this.Bind(openWebsiteButton.Rx().Tap(), ViewModel.OpenWebsiteAction);
        }
    }
}
