using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States.Pull
{
    public sealed class DetectGainingAccessToWorkspacesState : ISyncState<IFetchObservables>
    {
        private readonly IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace> dataSource;

        public StateResult<IFetchObservables> Continue { get; } = new StateResult<IFetchObservables>();
        public StateResult<IEnumerable<IWorkspace>> NewWorkspacesDetected { get; } = new StateResult<IEnumerable<IWorkspace>>();

        public DetectGainingAccessToWorkspacesState(IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace> dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            this.dataSource = dataSource;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => fetch.GetList<IWorkspace>()
                .CombineLatest(dataSource.GetAll(), workspacesWhichAreNotInDatabase)
                .Select(newWorkspaces =>
                    newWorkspaces.Any()
                        ? newWorkspacesDetected(newWorkspaces)
                        : continueTransition(fetch));

        private IEnumerable<IWorkspace> workspacesWhichAreNotInDatabase(IEnumerable<IWorkspace> fetched, IEnumerable<IWorkspace> stored)
            => fetched.Where(f => stored.None(s => s.Id == f.Id));

        private ITransition newWorkspacesDetected(IEnumerable<IWorkspace> workspaces)
            => NewWorkspacesDetected.Transition(workspaces);

        private ITransition continueTransition(IFetchObservables fetch)
            => Continue.Transition(fetch);
    }
}
