using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CoreGraphics;
using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Cells.Calendar;
using Toggl.Daneel.Views.Calendar;
using Toggl.Foundation.Calendar;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Extensions;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using UIKit;
using FoundationResources = Toggl.Foundation.Resources;

namespace Toggl.Daneel.ViewSources
{
    public sealed class CalendarCollectionViewSource : MvxCollectionViewSource, ICalendarCollectionViewLayoutDataSource
    {
        private static readonly string twelveHoursFormat = "h tt";
        private static readonly string twentyFourHoursFormat = "HH:mm";
        private static readonly string editingTwelveHoursFormat = "h:mm tt";
        private static readonly string editingTwentyFourHoursFormat = "HH:mm";

        private readonly string itemReuseIdentifier = nameof(CalendarItemView);
        private readonly string hourReuseIdentifier = nameof(HourSupplementaryView);
        private readonly string editingHourReuseIdentifier = nameof(HourSupplementaryView);
        private readonly string currentTimeReuseIdentifier = nameof(CurrentTimeSupplementaryView);

        private readonly IObservable<DateTime> date;
        private readonly IObservable<TimeFormat> timeOfDayFormatObservable;
        private readonly ObservableGroupedOrderedCollection<CalendarItem> collection;

        private IList<CalendarItem> calendarItems;
        private IList<CalendarCollectionViewItemLayoutAttributes> layoutAttributes;
        private TimeFormat timeOfDayFormat = TimeFormat.TwelveHoursFormat;
        private DateTime currentDate;
        private NSIndexPath editingItemIndexPath;

        private readonly CompositeDisposable disposeBag = new CompositeDisposable();
        private readonly ISubject<CalendarItem> itemTappedSubject = new Subject<CalendarItem>();

        private CalendarCollectionViewLayout layout => CollectionView.CollectionViewLayout as CalendarCollectionViewLayout;

        public bool IsEditing { get; private set; }

        public IObservable<CalendarItem> ItemTapped => itemTappedSubject.AsObservable();

        public CalendarCollectionViewSource(
            UICollectionView collectionView,
            IObservable<DateTime> date,
            IObservable<TimeFormat> timeOfDayFormat,
            ObservableGroupedOrderedCollection<CalendarItem> collection)
            : base(collectionView)
        {
            Ensure.Argument.IsNotNull(date, nameof(date));
            Ensure.Argument.IsNotNull(timeOfDayFormat, nameof(timeOfDayFormat));
            Ensure.Argument.IsNotNull(collection, nameof(collection));
            this.date = date;
            this.timeOfDayFormatObservable = timeOfDayFormat;
            this.collection = collection;

            calendarItems = new List<CalendarItem>();
            layoutAttributes = new List<CalendarCollectionViewItemLayoutAttributes>();

            registerCells();

            timeOfDayFormat
                .Subscribe(timeOfDayFormatChanged)
                .DisposedBy(disposeBag);

            collection
                .CollectionChange
                .ObserveOn(SynchronizationContext.Current)
                .VoidSubscribe(onCollectionChanges)
                .DisposedBy(disposeBag);

            date.Subscribe(dateChanged)
                .DisposedBy(disposeBag);

            onCollectionChanges();
        }

        protected override UICollectionViewCell GetOrCreateCellFor(UICollectionView collectionView, NSIndexPath indexPath, object item)
        {
            var cell = collectionView.DequeueReusableCell(itemReuseIdentifier, indexPath) as CalendarItemView;
            cell.Item = calendarItems[(int)indexPath.Item];
            cell.IsEditing = IsEditing && indexPath == editingItemIndexPath;
            return cell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
            => calendarItems.Count;

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var item = calendarItems[indexPath.Row];
            itemTappedSubject.OnNext(item);
        }

        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            if (elementKind == CalendarCollectionViewLayout.HourSupplementaryViewKind)
            {
                var reusableView = collectionView.DequeueReusableSupplementaryView(elementKind, hourReuseIdentifier, indexPath) as HourSupplementaryView;
                var hour = currentDate.AddHours((int)indexPath.Item);
                reusableView.SetLabel(hour.ToString(supplementaryHourFormat()));
                return reusableView;
            }
            else if (elementKind == CalendarCollectionViewLayout.EditingHourSupplementaryViewKind)
            {
                var reusableView = collectionView.DequeueReusableSupplementaryView(elementKind, editingHourReuseIdentifier, indexPath) as EditingHourSupplementaryView;
                var attrs = layoutAttributes[(int)editingItemIndexPath.Item];
                var hour = (int)indexPath.Item == 0 ? attrs.StartTime.ToLocalTime() : attrs.EndTime.ToLocalTime();
                reusableView.SetLabel(hour.ToString(editingHourFormat()));
                return reusableView;
            }

            return collectionView.DequeueReusableSupplementaryView(elementKind, currentTimeReuseIdentifier, indexPath);
        }

        public IEnumerable<NSIndexPath> IndexPathsOfCalendarItemsBetweenHours(int minHour, int maxHour)
        {
            if (calendarItems.None())
            {
                return Enumerable.Empty<NSIndexPath>();
            }

            var indices = calendarItems
                .Select((value, index) => new { value, index })
                .Where(t => t.value.StartTime.ToLocalTime().Hour >= minHour || t.value.EndTime.ToLocalTime().Hour <= maxHour)
                .Select(t => t.index);

            return indices.Select(index => NSIndexPath.FromItemSection(index, 0));
        }

        public CalendarCollectionViewItemLayoutAttributes LayoutAttributesForItemAtIndexPath(NSIndexPath indexPath)
            => layoutAttributes[(int)indexPath.Item];

        public NSIndexPath IndexPathForEditingItem()
            => editingItemIndexPath;

        public CalendarItem? CalendarItemAtPoint(CGPoint point)
        {
            var indexPath = CollectionView.IndexPathForItemAtPoint(point);
            if (indexPath != null && indexPath.Item < calendarItems.Count)
            {
                return calendarItems[(int)indexPath.Item];
            }
            return null;
        }

        public void StartEditing()
        {
            IsEditing = true;
            layout.IsEditing = true;
        }

        public void StartEditing(NSIndexPath indexPath)
        {
            IsEditing = true;
            layout.IsEditing = true;
            editingItemIndexPath = indexPath;
            CollectionView.ReloadItems(new NSIndexPath[] { indexPath });
        }

        public void StopEditing()
        {
            IsEditing = false;
            layout.IsEditing = false;
            layoutAttributes = calculateLayoutAttributes();
            layout.InvalidateLayoutForVisibleItems();
            editingItemIndexPath = null;
        }

        public NSIndexPath InsertItemView(DateTimeOffset startTime, TimeSpan duration)
        {
            if (!IsEditing)
                throw new InvalidOperationException("Set IsEditing before calling insert/update/remove");

            editingItemIndexPath = insertCalendarItem(startTime, duration);
            CollectionView.InsertItems(new NSIndexPath[] { editingItemIndexPath });
            return editingItemIndexPath;
        }

        public NSIndexPath UpdateItemView(NSIndexPath indexPath, DateTimeOffset startTime, TimeSpan duration)
        {
            if (!IsEditing)
                throw new InvalidOperationException("Set IsEditing before calling insert/update/remove");

            editingItemIndexPath = updateCalendarItem(indexPath, startTime, duration);

            updateEditingHours();
            layout.InvalidateLayoutForVisibleItems();

            return editingItemIndexPath;
        }

        public void RemoveItemView(NSIndexPath indexPath)
        {
            if (!IsEditing)
                throw new InvalidOperationException("Set IsEditing before calling insert/update/remove");

            removeCalendarItem(indexPath);
            CollectionView.DeleteItems(new NSIndexPath[] { indexPath });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            disposeBag.Dispose();
        }

        private void timeOfDayFormatChanged(TimeFormat timeFormat)
        {
            timeOfDayFormat = timeFormat;
            CollectionView.ReloadData();
        }

        private void onCollectionChanges()
        {
            calendarItems = collection.IsEmpty ? new List<CalendarItem>() : collection[0].ToList();
            layoutAttributes = calculateLayoutAttributes();
            CollectionView.ReloadData();
        }

        private void dateChanged(DateTime date)
        {
            this.currentDate = date;
        }

        private void registerCells()
        {
            CollectionView.RegisterNibForCell(CalendarItemView.Nib, itemReuseIdentifier);

            CollectionView.RegisterNibForSupplementaryView(HourSupplementaryView.Nib,
                                                           CalendarCollectionViewLayout.HourSupplementaryViewKind,
                                                           new NSString(hourReuseIdentifier));

            CollectionView.RegisterNibForSupplementaryView(EditingHourSupplementaryView.Nib,
                                                           CalendarCollectionViewLayout.EditingHourSupplementaryViewKind,
                                                           new NSString(editingHourReuseIdentifier));

            CollectionView.RegisterNibForSupplementaryView(CurrentTimeSupplementaryView.Nib,
                                                           CalendarCollectionViewLayout.CurrentTimeSupplementaryViewKind,
                                                           new NSString(currentTimeReuseIdentifier));
        }

        private IList<CalendarCollectionViewItemLayoutAttributes> calculateLayoutAttributes()
        {
            if (calendarItems.None())
                return new List<CalendarCollectionViewItemLayoutAttributes>();

            var initialValue = new List<List<(CalendarItem item, int index)>>() { new List<(CalendarItem item, int index)>() };

            var pairs = calendarItems
                .Select((item, index) => (item: item, index: index))
                .OrderBy(pair => pair.item.StartTime)
                .ToList();

            var attributes = pairs
                .Aggregate(initialValue, groupOverlappingItems)
                .Select(orderByEventFirst)
                .Select(toLayoutAttributes)
                .SelectMany(CommonFunctions.Identity)
                .OrderBy(pair => pair.index)
                .Select(pair => pair.attributes)
                .ToList();

            return attributes;

            List<List<(CalendarItem item, int index)>> groupOverlappingItems(
                List<List<(CalendarItem item, int index)>> previous,
                (CalendarItem item, int index) pair)
            {
                var group = previous.Last();
                if (group.None())
                {
                    group.Add(pair);
                }
                else
                {
                    var endTime = group.Max(i => i.item.EndTime.LocalDateTime);
                    if (pair.item.StartTime.LocalDateTime < endTime)
                        group.Add(pair);
                    else
                        previous.Add(new List<(CalendarItem, int)>() { pair });
                }

                return previous;
            }

            List<(CalendarItem item, int index)> orderByEventFirst(List<(CalendarItem item, int index)> group)
                => group.OrderBy((pair) => pair.item.Source == CalendarItemSource.Calendar, true).ToList();

            IEnumerable<(CalendarCollectionViewItemLayoutAttributes attributes, int index)> toLayoutAttributes(
                List<(CalendarItem item, int index)> group)
                => group.Select((pair, index) => (attributesForItem(index, pair.item, group.Count, index), pair.index));
        }

        private CalendarCollectionViewItemLayoutAttributes attributesForItem(
            int index,
            CalendarItem calendarItem,
            int overlappingItemsCount,
            int positionInOverlappingGroup)
        {
            var isEditing = IsEditing && editingItemIndexPath != null && index == editingItemIndexPath.Item;
            return new CalendarCollectionViewItemLayoutAttributes(
                calendarItem.StartTime.LocalDateTime,
                calendarItem.Duration,
                overlappingItemsCount,
                positionInOverlappingGroup,
                isEditing
            );
        }

        private NSIndexPath insertCalendarItem(DateTimeOffset startTime, TimeSpan duration)
        {
            var calendarItem = new CalendarItem("", CalendarItemSource.TimeEntry, startTime, duration, FoundationResources.NewTimeEntry, CalendarIconKind.None);

            calendarItems.Add(calendarItem);
            var position = calendarItems.Count - 1;

            layoutAttributes = calculateLayoutAttributes();

            var indexPath = NSIndexPath.FromItemSection(position, 0);
            return indexPath;
        }

        private NSIndexPath updateCalendarItem(NSIndexPath indexPath, DateTimeOffset startTime, TimeSpan duration)
        {
            var position = (int)indexPath.Item;

            calendarItems[position] = calendarItems[position]
                .WithStartTime(startTime)
                .WithDuration(duration);

            layoutAttributes = calculateLayoutAttributes();

            var updatedIndexPath = NSIndexPath.FromItemSection(position, 0);
            return updatedIndexPath;
        }

        private void removeCalendarItem(NSIndexPath indexPath)
        {
            calendarItems.RemoveAt((int)indexPath.Item);
            layoutAttributes = calculateLayoutAttributes();
        }

        private void updateEditingHours()
        {
            var startEditingHour = CollectionView
                .GetSupplementaryView(CalendarCollectionViewLayout.EditingHourSupplementaryViewKind, NSIndexPath.FromItemSection(0, 0)) as EditingHourSupplementaryView;
            var endEditingHour = CollectionView
                .GetSupplementaryView(CalendarCollectionViewLayout.EditingHourSupplementaryViewKind, NSIndexPath.FromItemSection(1, 0)) as EditingHourSupplementaryView;

            var attrs = layoutAttributes[(int)editingItemIndexPath.Item];
            if (startEditingHour != null)
            {
                var hour = attrs.StartTime.ToLocalTime();
                startEditingHour.SetLabel(hour.ToString(editingHourFormat()));
            }
            if (endEditingHour != null)
            {
                var hour = attrs.EndTime.ToLocalTime();
                endEditingHour.SetLabel(hour.ToString(editingHourFormat()));
            }
        }

        private string supplementaryHourFormat()
            => timeOfDayFormat.IsTwentyFourHoursFormat ? twentyFourHoursFormat : twelveHoursFormat;

        private string editingHourFormat()
            => timeOfDayFormat.IsTwentyFourHoursFormat ? editingTwentyFourHoursFormat : editingTwelveHoursFormat;
    }
}
