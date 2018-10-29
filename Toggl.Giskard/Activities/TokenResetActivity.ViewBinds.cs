using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using static Toggl.Foundation.Resources;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Toggl.Giskard.Activities
{
    public sealed partial class TokenResetActivity
    {
        private Toolbar toolbar;

        private TextView loginLabel;
        private EditText passwordEditText;
        private ProgressBar progressBar;
        private TextView resetMessageWarning;
        private TextView enterPasswordLabel;
        private TextView emailLabel;
        private TextView signoutLabel;
        private FloatingActionButton doneButton;

        protected override void InitializeViews()
        {
            toolbar = FindViewById<Toolbar>(Resource.Id.Toolbar);

            loginLabel = FindViewById<TextView>(Resource.Id.LoginLabel);
            passwordEditText = FindViewById<EditText>(Resource.Id.TokenResetPassword);
            progressBar = FindViewById<ProgressBar>(Resource.Id.TokenResetProgressBar);
            resetMessageWarning = FindViewById<TextView>(Resource.Id.TokenResetMessageWarning);
            enterPasswordLabel = FindViewById<TextView>(Resource.Id.TokenResetMessageEnterPasswordLabel);
            emailLabel = FindViewById<TextView>(Resource.Id.TokenResetEmailLabel);
            signoutLabel = FindViewById<TextView>(Resource.Id.TokenResetSignOutLabel);
            doneButton = FindViewById<FloatingActionButton>(Resource.Id.TokenResetDoneButton);
        }
    }
}
