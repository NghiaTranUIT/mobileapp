using MvvmCross.UI;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectableColorViewModel
    {
        public MvxColor Color { get; }

        public bool Selected { get; }

        public SelectableColorViewModel(MvxColor color, bool selected)
        {
            Ensure.Argument.IsNotNull(color, nameof(color));

            Color = color;
            Selected = selected;
        }
    }
}
