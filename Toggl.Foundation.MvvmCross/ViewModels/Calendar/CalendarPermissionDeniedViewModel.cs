using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels.Calendar
{
    [Preserve(AllMembers = true)]
    public sealed class CalendarPermissionDeniedViewModel : MvxViewModelResult<Unit>
    {
        private readonly IPermissionsService permissionsService;
        private readonly IMvxNavigationService navigationService;

        public UIAction EnableAccess { get; }

        public UIAction ContinueWithoutAccess { get; }

        public CalendarPermissionDeniedViewModel(
            IPermissionsService permissionsService,
            IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(permissionsService, nameof(permissionsService));

            this.navigationService = navigationService;
            this.permissionsService = permissionsService;

            EnableAccess = UIAction.FromAction(enableAccess);
            ContinueWithoutAccess = UIAction.FromAsync(continueWithoutAccessAction);
        }

        private Task continueWithoutAccessAction()
            => navigationService.Close(this, Unit.Default);

        private void enableAccess()
        {
            permissionsService.OpenAppSettings();
        }
    }
}
