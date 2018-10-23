using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Interactors
{
    public sealed class SetDefaultWorkspaceInteractor : IInteractor<IObservable<Unit>>
    {
        private readonly long workspaceId;
        private readonly ITimeService timeService;
        private readonly ISingletonDataSource<IThreadSafeUser> userDataSource;

        public SetDefaultWorkspaceInteractor(
            ITimeService timeService,
            ISingletonDataSource<IThreadSafeUser> userDataSource,
            long workspaceId)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(userDataSource, nameof(userDataSource));

            this.timeService = timeService;
            this.userDataSource = userDataSource;
            this.workspaceId = workspaceId;
        }

        public IObservable<Unit> Execute()
            => userDataSource
                .Current
                .FirstAsync()
                .Select(User.Builder.FromExisting)
                .Select(user => user
                    .SetDefaultWorkspaceId(workspaceId)
                    .SetSyncStatus(SyncStatus.SyncNeeded)
                    .SetAt(timeService.CurrentDateTime)
                    .Build())
                .SelectMany(userDataSource.Update)
                .SelectUnit();
    }
}
