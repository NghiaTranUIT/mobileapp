using Toggl.Foundation.Calendar;

namespace Toggl.Foundation.Extensions
{
    public static class CalendarItemExtensions
    {
        public static bool IsEditable(this CalendarItem calendarItem)
            => calendarItem.Source == CalendarItemSource.TimeEntry;
    }
}
