using MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.Extensions
{
    public static class ViewModelExtensions
    {
        public static UIAction Close(this MvxViewModel viewModel)
            => UIAction.FromAsync(() => viewModel.NavigationService.Close(viewModel));

        public static UIAction Close<TResult>(this MvxViewModelResult<TResult> viewModel, TResult result)
            => UIAction.FromAsync(() => viewModel.NavigationService.Close(viewModel, result));
    }
}
