using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels.Calendar
{
    public abstract class SelectUserCalendarsViewModelBase : MvxViewModel<bool, string[]>
    {
        private readonly IUserPreferences userPreferences;

        protected bool ForceItemSelection { get; private set; }

        protected IInteractorFactory InteractorFactory { get; }

        protected HashSet<string> SelectedCalendarIds { get; } = new HashSet<string>();

        public ObservableGroupedOrderedCollection<SelectableUserCalendarViewModel> Calendars { get; }
            = new ObservableGroupedOrderedCollection<SelectableUserCalendarViewModel>(
                indexKey: c => c.Id,
                orderingKey: c => c.Name,
                groupingKey: c => c.SourceName
            );

        public InputAction<SelectableUserCalendarViewModel> SelectCalendarAction { get; }

        protected SelectUserCalendarsViewModelBase(
            IUserPreferences userPreferences, 
            IInteractorFactory interactorFactory)
        {
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.userPreferences = userPreferences;
            InteractorFactory = interactorFactory;

            SelectCalendarAction = new InputAction<SelectableUserCalendarViewModel>(selectCalendar);
        }

        public override sealed void Prepare(bool parameter)
        {
            ForceItemSelection = parameter;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            SelectedCalendarIds.AddRange(userPreferences.EnabledCalendarIds());

            await InteractorFactory
                .GetUserCalendars()
                .Execute()
                .Catch((NotAuthorizedException _) => Observable.Return(new List<UserCalendar>()))
                .Select(calendars => calendars.Select(toSelectable))
                .Do(calendars => calendars.ForEach(calendar => Calendars.InsertItem(calendar)));
        }

        private SelectableUserCalendarViewModel toSelectable(UserCalendar calendar)
            => new SelectableUserCalendarViewModel(calendar, SelectedCalendarIds.Contains(calendar.Id));

        private IObservable<Unit> selectCalendar(SelectableUserCalendarViewModel calendar)
        {
            if (SelectedCalendarIds.Contains(calendar.Id))
                SelectedCalendarIds.Remove(calendar.Id);
            else
                SelectedCalendarIds.Add(calendar.Id);

            calendar.Selected = !calendar.Selected;

            return Observable.Return(Unit.Default);
        }
    }
}
