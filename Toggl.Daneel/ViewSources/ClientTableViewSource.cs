using System;
using System.Reactive.Subjects;
using Foundation;
using MvvmCross.Binding.Extensions;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Cells;
using Toggl.Daneel.Views;
using Toggl.Daneel.Views.EntityCreation;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ClientTableViewSource : ReactiveSectionedListTableViewSource<SelectableClientViewModel, ClientViewCell>
    {
        public bool SuggestCreation { get; set; }
        public string SuggestCreationName { get; set; }

        private const int rowHeight = 48;

        private const string cellIdentifier = nameof(ClientViewCell);
        private const string createEntityCellIdentifier = nameof(CreateEntityViewCell);


        public ClientTableViewSource(UITableView tableView,
            ObservableGroupedOrderedCollection<SelectableClientViewModel> collection)
            : base(collection, cellIdentifier)
        {
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RegisterNibForCellReuse(ClientViewCell.Nib, cellIdentifier);
            tableView.RegisterNibForCellReuse(CreateEntityViewCell.Nib, createEntityCellIdentifier);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (SuggestCreation && indexPath.Row == 0)
            {
                var cell = tableView.DequeueReusableCell(createEntityCellIdentifier, indexPath) as CreateEntityViewCell;
                cell.Item = $"Create client \"{SuggestCreationName?.Trim() ?? ""}\"";
                return cell;
            }

            return base.GetCell(tableView, indexPath);
        }

        public override nint RowsInSection(UITableView tableview, nint section) =>
            base.RowsInSection(tableview, section) + (SuggestCreation ? 1 : 0);

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;

        public override void RefreshHeader(UITableView tableView, int section)
        {
        }
    }
}