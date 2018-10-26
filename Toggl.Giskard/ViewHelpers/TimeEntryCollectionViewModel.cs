using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.MvvmCross.Transformations;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.ViewHelpers
{
    public class TimeEntryCollectionViewModel
    {
        private DateTimeOffset headerDate;
        private string durationText;

        public TimeEntryCollectionViewModel(IReadOnlyList<TimeEntryViewModel> timeEntryViewModels)
        {
            var firstItem = timeEntryViewModels.First();
            headerDate = firstItem.StartTime;

            var totalDuration = timeEntryViewModels.Sum(vm => vm.Duration);
            durationText = totalDuration.ToFormattedString(firstItem.DurationFormat);
        }

        public string DurationText => durationText;

        public string HeaderDate(DateTimeOffset now) => DateToTitleString.Convert(headerDate, now);
    }
}
