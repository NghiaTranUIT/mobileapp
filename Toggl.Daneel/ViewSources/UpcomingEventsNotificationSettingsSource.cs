using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Foundation;
using Toggl.Daneel.Cells.Settings;
using Toggl.Foundation.Extensions;
using Toggl.Multivac;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class UpcomingEventsNotificationSettingsSource : UITableViewSource
    {
        private const int rowHeight = 44;
        private const string cellIdentifier = nameof(UpcomingEventsOptionCell);

        private readonly ISubject<CalendarNotificationsOption> selectedOptionSubject;

        public IObservable<CalendarNotificationsOption> SelectedOptionChanged { get; }

        public IList<CalendarNotificationsOption> AvailableOptions { get; private set; }

        public UpcomingEventsNotificationSettingsSource(UITableView tableView, IList<CalendarNotificationsOption> options)
        {
            selectedOptionSubject = new Subject<CalendarNotificationsOption>();
            SelectedOptionChanged = selectedOptionSubject.AsObservable().DistinctUntilChanged();
            AvailableOptions = options;
            tableView.RegisterNibForCellReuse(UpcomingEventsOptionCell.Nib, cellIdentifier);
        }

        public override nint RowsInSection(UITableView tableview, nint section)
            => AvailableOptions.Count;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as UpcomingEventsOptionCell;
            cell.Text = AvailableOptions[indexPath.Row].Title();
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            selectedOptionSubject.OnNext(AvailableOptions[indexPath.Row]);
        }
    }
}
