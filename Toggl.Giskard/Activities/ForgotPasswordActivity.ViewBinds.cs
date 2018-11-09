using Android.Widget;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Activities
{
    public partial class ForgotPasswordActivity
    {
        private TextInputLayoutWithHelperText loginEmail;
        private EditText loginEmailEditText;
        private Button resetPasswordButton;
        private ProgressBar loadingProgressBar;

        protected override void InitializeViews()
        {
            loginEmail = FindViewById<TextInputLayoutWithHelperText>(Resource.Id.LoginEmail);
            loginEmailEditText = FindViewById<EditText>(Resource.Id.LoginEmailEditText);
            resetPasswordButton = FindViewById<Button>(Resource.Id.ResetPasswordButton);
            loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.LoadingProgressBar);
        }
    }
}
