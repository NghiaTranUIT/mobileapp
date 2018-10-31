using System;
using MvvmCross.Binding.Combiners;

namespace Toggl.Foundation.MvvmCross.Combiners
{
    public sealed class ShowTagsValueCombiner : BaseTwoValuesCombiner<bool, bool>
    {
        public ShowTagsValueCombiner()
        {
        }

        protected override object Combine(bool isInaccessible, bool hasTags)
            => !isInaccessible || hasTags;
    }
}
