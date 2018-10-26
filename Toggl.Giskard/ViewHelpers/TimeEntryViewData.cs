using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Toggl.Foundation.MvvmCross.Transformations;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using static Toggl.Foundation.Helper.Color;

namespace Toggl.Giskard.ViewHelpers
{
    public class TimeEntryViewData
    {
        public TimeEntryViewModel TimeEntryViewModel { get; }
        public ISpannable ProjectTaskClientText { get; }
        public string DurationText { get; }

        public ViewStates ProjectTaskClientVisibility { get; }
        public ViewStates HasTagsIconVisibility { get; }
        public ViewStates BillableIconVisibility { get; }
        public ViewStates ContinueButtonVisibility { get; }
        public ViewStates ErrorNeedsSyncVisibility { get; }
        public ViewStates ErrorImageViewVisibility { get; }
        public ViewStates ContinueImageVisibility { get; }
        public ViewStates AddDescriptionLabelVisibility { get; }
        public ViewStates DescriptionVisibility { get; }

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

            DurationText = TimeEntryViewModel.Duration.HasValue
                ? DurationAndFormatToString.Convert(TimeEntryViewModel.Duration.Value, TimeEntryViewModel.DurationFormat)
                : "";

            DescriptionVisibility = TimeEntryViewModel.HasDescription.ToVisibility();
            AddDescriptionLabelVisibility = (!TimeEntryViewModel.HasDescription).ToVisibility();
            ContinueImageVisibility = TimeEntryViewModel.CanContinue.ToVisibility();
            ErrorImageViewVisibility = (!TimeEntryViewModel.CanContinue).ToVisibility();
            ErrorNeedsSyncVisibility = TimeEntryViewModel.NeedsSync.ToVisibility();
            ContinueButtonVisibility = TimeEntryViewModel.CanContinue.ToVisibility();
            BillableIconVisibility = TimeEntryViewModel.IsBillable.ToVisibility();
            HasTagsIconVisibility = TimeEntryViewModel.HasTags.ToVisibility();
        }
    }
}
