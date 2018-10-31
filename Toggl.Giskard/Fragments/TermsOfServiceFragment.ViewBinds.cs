using Android.Views;
using Android.Widget;

namespace Toggl.Giskard.Fragments
{
    public sealed partial class TermsOfServiceFragment
    {
        private TextView privacyPolicyTextView;
        private TextView termsOfServiceTextView;
        private Button acceptButton;

        public void InitializeViews(View fragmentView)
        {
            privacyPolicyTextView = fragmentView.FindViewById<TextView>(Resource.Id.ViewPrivacyPolicyTextView);
            termsOfServiceTextView = fragmentView.FindViewById<TextView>(Resource.Id.ViewTermsOfServiceTextView);
            acceptButton = fragmentView.FindViewById<Button>(Resource.Id.AcceptButton);
        }
    }
}
