using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Toggl.Foundation.MvvmCross.ViewModels;
using static Toggl.Foundation.Helper.Color;

namespace Toggl.Giskard.ViewHelpers
{
    public class TimeEntryViewData
    {
        public TimeEntryViewModel TimeEntryViewModel { get; }
        public ISpannable ProjectTaskClientText { get; }
        public ViewStates ProjectTaskClientVisibility { get; }

        public TimeEntryViewData(TimeEntryViewModel timeEntryViewModel)
        {
            TimeEntryViewModel = timeEntryViewModel;

            var spannableString = new SpannableStringBuilder();
            if (TimeEntryViewModel.HasProject)
            {
                spannableString.Append(TimeEntryViewModel.ProjectName, new ForegroundColorSpan(Color.ParseColor(TimeEntryViewModel.ProjectColor)), SpanTypes.ExclusiveInclusive);

                if (!string.IsNullOrEmpty(TimeEntryViewModel.TaskName))
                {
                    spannableString.Append($": {TimeEntryViewModel.TaskName}");
                }

                if (!string.IsNullOrEmpty(TimeEntryViewModel.ClientName))
                {
                    spannableString.Append($" {TimeEntryViewModel.ClientName}", new ForegroundColorSpan(Color.ParseColor(ClientNameColor)), SpanTypes.ExclusiveExclusive);
                }

                ProjectTaskClientText = spannableString;
                ProjectTaskClientVisibility = ViewStates.Visible;
            }
            else
            {
                ProjectTaskClientText = new SpannableString(string.Empty);
                ProjectTaskClientVisibility = ViewStates.Gone;
            }
        }
    }
}
