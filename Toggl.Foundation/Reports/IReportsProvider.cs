using System;
using Toggl.Multivac.Models.Reports;

namespace Toggl.Foundation.Reports
{
    public interface IReportsProvider
    {
        IObservable<ProjectSummaryReport> GetProjectSummary(
            long workspaceId, DateTimeOffset startDate, DateTimeOffset? endDate);

        IObservable<ITimeEntriesTotals> GetTotals(
            long userId, long workspaceId, DateTimeOffset startDate, DateTimeOffset endDate);
    }
}
