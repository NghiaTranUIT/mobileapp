using Toggl.Multivac.Models;
using Toggl.Foundation.Services;

namespace Toggl.Giskard.Services
{
    public class NoopIntentDonationServiceAndroid: IIntentDonationService
    {
        public void SetDefaultShortcutSuggestions(IWorkspace workspace)
        {
        }

        public void DonateStartTimeEntry(IWorkspace workspace, ITimeEntry timeEntry)
        {
        }

        public void DonateStopCurrentTimeEntry()
        {
        }

        public void DonateShowReport(ReportPeriod period)
        {
        }

        public void DonateShowReport()
        {
        }

        public void ClearAll()
        {
        }
    }
}