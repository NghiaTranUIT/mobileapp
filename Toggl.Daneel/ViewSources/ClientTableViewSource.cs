using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Foundation;
using Toggl.Daneel.Cells;
using Toggl.Daneel.Views.Client;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ClientTableViewSource : ReactiveSectionedListTableViewSource<SelectableClientViewModel, ClientViewCell>
    {
        private const int rowHeight = 48;

        private const string cellIdentifier = nameof(ClientViewCell);
        private const string createEntityCellIdentifier = nameof(CreateClientViewCell);

        public ClientTableViewSource(UITableView tableView,
            ObservableGroupedOrderedCollection<SelectableClientViewModel> collection)
            : base(collection, cellIdentifier)
        {
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RegisterNibForCellReuse(ClientViewCell.Nib, cellIdentifier);
            tableView.RegisterNibForCellReuse(CreateClientViewCell.Nib, createEntityCellIdentifier);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = DisplayedItems[indexPath.Section][indexPath.Row];
            var identifier = item.IsCreation ? createEntityCellIdentifier : cellIdentifier;
            var cell = tableView.DequeueReusableCell(identifier) as BaseTableViewCell<SelectableClientViewModel>;
            cell.Item = item;
            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;

        public override void RefreshHeader(UITableView tableView, int section)
        {
        }
    }
}