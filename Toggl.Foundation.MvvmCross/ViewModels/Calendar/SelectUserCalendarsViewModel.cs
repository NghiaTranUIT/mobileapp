using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using Toggl.Foundation.Interactors;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels.Calendar
{
    [Preserve(AllMembers = true)]
    public sealed class SelectUserCalendarsViewModel : SelectUserCalendarsViewModelBase
    {
        private readonly IMvxNavigationService navigationService;

        public UIAction DoneAction { get; private set; }

        public SelectUserCalendarsViewModel(
            IUserPreferences userPreferences,
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService) 
            : base(userPreferences, interactorFactory)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;
        }

        public override Task Initialize()
        {
            var enabledObservable = ForceItemSelection
                ? SelectCalendarAction.Elements
                    .Select(_ => SelectedCalendarIds.Any())
                    .DistinctUntilChanged()
                : Observable.Return(true);

            DoneAction = UIAction.FromAsync(done, enabledObservable);

            return base.Initialize();
        }

        private Task done()
            => navigationService.Close(this, SelectedCalendarIds.ToArray());
    }
}
