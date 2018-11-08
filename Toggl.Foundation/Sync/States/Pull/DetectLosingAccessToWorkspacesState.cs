using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States.Pull
{
    public sealed class DetectLosingAccessToWorkspacesState : ISyncState<IFetchObservables>
    {
        private readonly IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace> dataSource;
        private readonly IAnalyticsService analyticsService;

        public StateResult<IFetchObservables> Continue { get; } = new StateResult<IFetchObservables>();

        public DetectLosingAccessToWorkspacesState(IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace> dataSource, IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.dataSource = dataSource;
            this.analyticsService = analyticsService;
        }

        public IObservable<ITransition> Start(IFetchObservables fetchObservables)
            => fetchObservables.GetList<IWorkspace>()
                .SelectMany(workspacesWhichWereNotFetched)
                .SelectMany(markAsInaccessible)
                .SelectValue(Continue.Transition(fetchObservables));

        private IObservable<IList<IThreadSafeWorkspace>> workspacesWhichWereNotFetched(List<IWorkspace> fetchedWorkspaces)
            => allStoredWorkspaces()
                .Where(stored => fetchedWorkspaces.None(fetched => fetched.Id == stored.Id))
                .ToList()
                .Do(trackLoseOfWorkspaceAccessIfNeeded);

        private IObservable<IThreadSafeWorkspace> allStoredWorkspaces()
            => dataSource.GetAll(ws => ws.Id > 0 && ws.IsInaccessible == false)
                         .SelectMany(CommonFunctions.Identity);

        private IObservable<Unit> markAsInaccessible(IList<IThreadSafeWorkspace> workspacesToMark)
            => Observable.Return(workspacesToMark)
                .SelectMany(CommonFunctions.Identity)
                .SelectMany(markAsInaccessible)
                .ToList()
                .SelectValue(Unit.Default);

        private IObservable<IThreadSafeWorkspace> markAsInaccessible(IThreadSafeWorkspace workspaceToMark)
            => dataSource.Update(workspaceToMark.AsInaccessible());

        private void trackLoseOfWorkspaceAccessIfNeeded(IList<IThreadSafeWorkspace> workspacesNotFetched)
        {
            if (workspacesNotFetched.Count > 0)
            {
                analyticsService.LostWorkspaceAccess.Track();
            }
        }
    }
}
