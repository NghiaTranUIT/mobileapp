using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Foundation;
using Toggl.Daneel.Cells;
using UIKit;


namespace Toggl.Daneel.ViewSources
{
    public class ListCollectionViewSource<TModel, TCell> : UICollectionViewSource
        where TCell : BaseCollectionViewCell<TModel>
    {
        private readonly string cellIdentifier;
        internal IImmutableList<TModel> items;

        public EventHandler<TModel> OnItemTapped { get; set; }

        public ListCollectionViewSource(IImmutableList<TModel> items, string cellIdentifier)
        {
            this.items = items;
            this.cellIdentifier = cellIdentifier;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(cellIdentifier, indexPath) as BaseCollectionViewCell<TModel>;
            cell.Item = items[indexPath.Row];
            return cell;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            collectionView.DeselectItem(indexPath, true);
            OnItemTapped.Invoke(this, items[indexPath.Row]);
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
            => items.Count;
    }
}
