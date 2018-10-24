using System;
using Toggl.Multivac.Models.Reports;

namespace Toggl.Ultrawave.ApiClients.Interfaces
{
    public interface ITimeEntriesReportsApi
    {
        IObservable<ITimeEntriesTotals> GetTotals(
            long userId, long workspaceId, DateTimeOffset startDate, DateTimeOffset endDate);
    }
}
