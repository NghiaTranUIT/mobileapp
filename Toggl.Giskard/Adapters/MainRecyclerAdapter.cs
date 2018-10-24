using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Support.V7.Widget;
using Android.Views;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.ViewHolders;
using Toggl.Multivac.Extensions;
using Toggl.Foundation;

namespace Toggl.Giskard.Adapters
{
    public class MainRecyclerAdapter : ReactiveSectionedRecyclerAdapter<TimeEntryViewModel, MainLogCellViewHolder, MainLogSectionViewHolder>
    {
        public const int SuggestionViewType = 2;

        private readonly ITimeService timeService;

        public IObservable<TimeEntryViewModel> TimeEntryTaps
            => timeEntryTappedSubject.AsObservable();

        public IObservable<TimeEntryViewModel> ContinueTimeEntrySubject
            => continueTimeEntrySubject.AsObservable();

        public IObservable<TimeEntryViewModel> DeleteTimeEntrySubject
            => deleteTimeEntrySubject.AsObservable();

        public SuggestionsViewModel SuggestionsViewModel { get; set; }

        private Subject<TimeEntryViewModel> timeEntryTappedSubject = new Subject<TimeEntryViewModel>();
        private Subject<TimeEntryViewModel> continueTimeEntrySubject = new Subject<TimeEntryViewModel>();
        private Subject<TimeEntryViewModel> deleteTimeEntrySubject = new Subject<TimeEntryViewModel>();

        public MainRecyclerAdapter(
            ObservableGroupedOrderedCollection<TimeEntryViewModel> items,
            ITimeService timeService)
            : base(items)
        {
            this.timeService = timeService;
        }

        public void ContinueTimeEntry(int position)
        {
            var continuedTimeEntry = getItemAt(position);
            NotifyItemChanged(position);
            continueTimeEntrySubject.OnNext(continuedTimeEntry);
        }

        public void DeleteTimeEntry(int position)
        {
            var deletedTimeEntry = getItemAt(position);
            deleteTimeEntrySubject.OnNext(deletedTimeEntry);
        }

        public override int HeaderOffset => 1;

        protected override bool TryBindCustomViewType(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is MainLogSuggestionsListViewHolder suggestionsViewHolder)
            {
                suggestionsViewHolder.UpdateView();
                return true;
            }

            return false;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType == SuggestionViewType)
            {
                var suggestionsView = LayoutInflater.FromContext(parent.Context).Inflate(Resource.Layout.MainSuggestions, parent, false);
                return new MainLogSuggestionsListViewHolder(suggestionsView, SuggestionsViewModel);
            }

            return base.OnCreateViewHolder(parent, viewType);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is MainLogSectionViewHolder mainLogHeader)
            {
                mainLogHeader.Now = timeService.CurrentDateTime;
            }

            base.OnBindViewHolder(holder, position);
        }

        public override int GetItemViewType(int position)
        {
            if (position == 0)
            {
                return SuggestionViewType;
            }

            return base.GetItemViewType(position);
        }

        protected override MainLogSectionViewHolder CreateHeaderViewHolder(ViewGroup parent)
        {
            var header = new MainLogSectionViewHolder(LayoutInflater.FromContext(parent.Context)
                .Inflate(Resource.Layout.MainLogHeader, parent, false));
            header.Now = timeService.CurrentDateTime;
            return header;
        }

        protected override MainLogCellViewHolder CreateItemViewHolder(ViewGroup parent)
        {
            return new MainLogCellViewHolder(LayoutInflater.FromContext(parent.Context).Inflate(Resource.Layout.MainLogCell, parent, false))
            {
                TappedSubject = timeEntryTappedSubject,
                ContinueButtonTappedSubject = continueTimeEntrySubject
            };
        }

        protected override long IdFor(TimeEntryViewModel item)
            => item.Id;

        protected override long IdForSection(IReadOnlyList<TimeEntryViewModel> section)
            => section.First().StartTime.Date.GetHashCode();

        protected override bool AreItemContentsTheSame(TimeEntryViewModel item1, TimeEntryViewModel item2)
            => item1 == item2;

        protected override bool AreSectionsRepresentationsTheSame(IReadOnlyList<TimeEntryViewModel> one, IReadOnlyList<TimeEntryViewModel> other)
        {
            var oneFirst = one.FirstOrDefault()?.StartTime.Date;
            var otherFirst = other.FirstOrDefault()?.StartTime.Date;
            return (oneFirst != null || otherFirst != null)
                   && oneFirst == otherFirst
                   && one.ContainsExactlyAll(other);
        }
    }
}
