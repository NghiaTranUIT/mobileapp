using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Foundation;
using Toggl.Daneel.Views;
using Toggl.Daneel.Views.EntityCreation;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ClientTableViewSource : ReactiveSectionedListTableViewSource<SelectableClientViewModel, ClientViewCell>
    {
        public BehaviorSubject<bool> SuggestCreation { get; } = new BehaviorSubject<bool>(false);
        public BehaviorSubject<string> SuggestCreationName { get; } = new BehaviorSubject<string>("");
        public IObservable<string> SuggestCreationTapped { get; }

        private ISubject<string> suggestCreationTapped = new Subject<string>();
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
            SuggestCreationTapped = suggestCreationTapped.AsObservable();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (!SuggestCreation.Value) return base.GetCell(tableView, indexPath);

            if (indexPath.Row == 0)
            {
                var creationCell = tableView.DequeueReusableCell(createEntityCellIdentifier, indexPath) as CreateEntityViewCell;
                creationCell.Item = $"Create client \"{SuggestCreationName.Value.Trim()}\"";
                return creationCell;
            }

            var cell = tableView.DequeueReusableCell(cellIdentifier, indexPath) as ClientViewCell;
            cell.Item = DisplayedItems[indexPath.Section][indexPath.Row - 1];
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var numberOfCreationSuggestionRow = SuggestCreation.Value ? 1 : 0;
            if (DisplayedItems.Count == 0)
            {
                return numberOfCreationSuggestionRow;
            }

            return base.RowsInSection(tableview, section) + numberOfCreationSuggestionRow;
        }

        public override nint NumberOfSections(UITableView tableView) => 1;

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (!SuggestCreation.Value)
            {
                base.RowSelected(tableView, indexPath);
                return;
            }

            if (indexPath.Row == 0)
            {
                suggestCreationTapped.OnNext(SuggestCreationName.Value);
                return;
            }

            base.RowSelected(tableView, NSIndexPath.FromRowSection(indexPath.Row - 1, indexPath.Section));
        }

        public override void RefreshHeader(UITableView tableView, int section)
        {
        }
    }
}