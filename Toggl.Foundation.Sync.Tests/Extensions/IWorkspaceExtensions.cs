using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.Tests.Extensions
{
    public static class IWorkspaceExtensions
    {
        public static IThreadSafeWorkspace ToSyncable(
            this IWorkspace workspace)
            => new MockWorkspace
            {
                Admin = workspace.Admin,
                At = workspace.At,
                DefaultCurrency = workspace.DefaultCurrency,
                DefaultHourlyRate = workspace.DefaultHourlyRate,
                Id = workspace.Id,
                IsDeleted = false,
                IsInaccessible = false,
                LastSyncErrorMessage = null,
                LogoUrl = workspace.LogoUrl,
                Name = workspace.Name,
            };
    }
}
