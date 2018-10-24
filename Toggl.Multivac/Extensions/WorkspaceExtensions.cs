using Toggl.Multivac.Models;

namespace Toggl.Multivac.Extensions
{
    public static class WorkspaceExtensions
    {
        public static bool IsEligibleForProjectCreation(this IWorkspace self) =>
            self.Admin || !self.OnlyAdminsMayCreateProjects;
    }
}