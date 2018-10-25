using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.Tests.Extensions
{
    public static class IPreferencesExtensions
    {
        public static IThreadSafePreferences ToSyncable(this IPreferences preferences)
            => new MockPreferences
            {
                CollapseTimeEntries = preferences.CollapseTimeEntries,
                DateFormat = preferences.DateFormat,
                DurationFormat = preferences.DurationFormat,
                Id = 0,
                IsDeleted = false,
                LastSyncErrorMessage = null,
                SyncStatus = SyncStatus.InSync,
                TimeOfDayFormat = preferences.TimeOfDayFormat
            };
    }
}
