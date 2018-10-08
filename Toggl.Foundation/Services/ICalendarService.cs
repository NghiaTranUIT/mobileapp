using System;
using System.Collections.Generic;
using Toggl.Foundation.Calendar;
using Toggl.Multivac;

namespace Toggl.Foundation.Services
{
    public interface ICalendarService
    {
        IObservable<CalendarItem> GetEventWithId(string id);

        IObservable<IEnumerable<CalendarItem>> GetEventsForDate(DateTime date);

        IObservable<IEnumerable<CalendarItem>> GetEventsInRange(DateTimeOffset start, DateTimeOffset end);

        IObservable<IEnumerable<UserCalendar>> GetUserCalendars();
    }
}
