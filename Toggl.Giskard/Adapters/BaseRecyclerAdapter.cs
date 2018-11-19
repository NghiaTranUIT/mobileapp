using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Android.Runtime;
using Android.Support.V7.Util;
using Android.Support.V7.Widget;
using Android.Views;
using Toggl.Giskard.ViewHolders;

namespace Toggl.Giskard.Adapters
{
    public abstract class BaseRecyclerAdapter<T> : RecyclerView.Adapter where T: IEquatable<T>
    {
        private Subject<T> itemTapSubject = new Subject<T>();
        public IObservable<T> ItemTapObservable => itemTapSubject.AsObservable();

        public Func<T, Task> OnItemTapped { get; set; }

        private IList<T> items = new List<T>();
        public virtual IList<T> Items
        {
            get => items;
            set => SetItems(value ?? new List<T>());
        }

        protected BaseRecyclerAdapter()
        {
        }

        protected BaseRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(parent.Context);

            var viewHolder = CreateViewHolder(parent, inflater, viewType);
            viewHolder.Tapped = async item =>
            {
                itemTapSubject.OnNext(item);
                await OnItemTapped(item);
            };

            return viewHolder;
        }

        protected abstract BaseRecyclerViewHolder<T> CreateViewHolder(ViewGroup parent, LayoutInflater inflater,
            int viewType);

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = GetItem(position);
            ((BaseRecyclerViewHolder<T>)holder).Item = item;
        }

        public override int ItemCount => items.Count;

        public virtual T GetItem(int viewPosition)
            => items[viewPosition];

        protected virtual void SetItems(IList<T> items)
        {
            DiffUtil.DiffResult diffResult = DiffUtil.CalculateDiff(new BaseDiffCallBack(this.items, items));
            diffResult.DispatchUpdatesTo(this);
        }

        private sealed class BaseDiffCallBack : DiffUtil.Callback
        {
            private IList<T> oldItems;
            private IList<T> newItems;

            public BaseDiffCallBack(IList<T> oldItems, IList<T> newItems)
            {
                this.oldItems = oldItems;
                this.newItems = newItems;
            }

            public override bool AreContentsTheSame(int oldItemPosition, int newItemPosition)
            {
                var oldItem = oldItems[oldItemPosition];
                var newItem = newItems[newItemPosition];
                return oldItem.Equals(newItem);
            }

            public override bool AreItemsTheSame(int oldItemPosition, int newItemPosition)
            {
                return ReferenceEquals(oldItems[oldItemPosition], newItems[newItemPosition]);
            }

            public override int NewListSize => oldItems.Count;
            public override int OldListSize => newItems.Count;
        }
    }
}
