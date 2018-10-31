using System;
using System.Collections.Generic;
using Toggl.Foundation.Calendar;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;

namespace Toggl.Giskard.Services
{
    public sealed class CalendarServiceAndroid : PermissionAwareCalendarService
    {
        public CalendarServiceAndroid(IPermissionsService permissionsService) 
            : base(permissionsService)
        {
        }

        protected override IEnumerable<UserCalendar> NativeGetUserCalendars()
            => new List<UserCalendar>();

        protected override IEnumerable<CalendarItem> NativeGetEventsInRange(DateTimeOffset start, DateTimeOffset end)
            => new List<CalendarItem>();

        protected override CalendarItem NativeGetCalendarWithId(string id)
            => default(CalendarItem);
    }
}
