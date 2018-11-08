using Android.Support.V7.Widget;
using Android.Widget;

namespace Toggl.Giskard.Activities
{
    public partial class SelectClientActivity
    {
        private ImageView backImageView;
        private EditText filterEditText;
        private RecyclerView selectClientRecyclerView;

        protected override void InitializeViews()
        {
            backImageView = FindViewById<ImageView>(Resource.Id.BackImageView);
            filterEditText = FindViewById<EditText>(Resource.Id.FilterEditText);
            selectClientRecyclerView = FindViewById<RecyclerView>(Resource.Id.SelectClientRecyclerView);
        }
    }
}