using MvvmCross.ViewModels;
using MvvmCross.UI;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectableColorViewModel : MvxNotifyPropertyChanged
    {
        public MvxColor Color { get; }
        public bool Selected { get; }

        public SelectableColorViewModel(MvxColor color, bool selected)
        {
            Ensure.Argument.IsNotNull(color, nameof(color));

            Color = color;
            Selected = selected;
        }

        public SelectableColorViewModel WithColor(MvxColor color)
            => new SelectableColorViewModel(color, Selected);

        public SelectableColorViewModel Select(bool isSelected = true)
            => new SelectableColorViewModel(Color, isSelected);

        public bool IsSameColorAs(SelectableColorViewModel viewModel)
            => viewModel.Color.ARGB == Color.ARGB;
    }
}
