using System;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Suggestions;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.ViewHelpers;
using Toggl.Multivac.Extensions;
using TogglResources = Toggl.Foundation.Resources;

namespace Toggl.Giskard.ViewHolders
{
    public class MainLogUserFeedbackViewHolder : RecyclerView.ViewHolder
    {
        private RatingViewModel ratingViewModel;

        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        public MainLogUserFeedbackViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public MainLogUserFeedbackViewHolder(View itemView, RatingViewModel ratingViewModel) : base(itemView)
        {
            this.ratingViewModel = ratingViewModel;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            disposeBag.Dispose();
        }
    }
}
