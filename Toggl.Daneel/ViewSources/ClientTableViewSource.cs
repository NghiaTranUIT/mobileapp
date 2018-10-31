using System;
using Foundation;
using MvvmCross.Binding.Extensions;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Views;
using Toggl.Daneel.Views.EntityCreation;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ClientTableViewSource : ReactiveSectionedListTableViewSource<SelectableClientViewModel, ClientViewCell>
    {

        private const int rowHeight = 48;

        private const string cellIdentifier = nameof(ClientViewCell);
        private const string createEntityCellIdentifier = nameof(CreateEntityViewCell);

        public bool SuggestCreation { get; set; }

        public ClientTableViewSource(UITableView tableView,
            ObservableGroupedOrderedCollection<SelectableClientViewModel> collection)
            : base(collection, cellIdentifier)
        {
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RegisterNibForCellReuse(ClientViewCell.Nib, cellIdentifier);
            tableView.RegisterNibForHeaderFooterViewReuse(CreateEntityViewCell.Nib, createEntityCellIdentifier);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;

        public override nfloat GetHeightForHeader(UITableView tableView, nint section) => rowHeight;

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var header = (CreateEntityViewCell) tableView.DequeueReusableHeaderFooterView(createEntityCellIdentifier);
            header.Item = "CREATE HEADER";
            return header;
        }

        public override void RefreshHeader(UITableView tableView, int section)
        {
        }
    }
}