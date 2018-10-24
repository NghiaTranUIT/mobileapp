using System;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using System.Linq;
using System.Collections.Generic;
using Toggl.Foundation.Exceptions;
using System.Collections.Immutable;
using MvvmCross.Navigation;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectDefaultWorkspaceViewModel : MvxViewModel
    {
        private readonly ITogglDataSource dataSource;
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;

        public IImmutableList<SelectableWorkspaceViewModel> Workspaces { get; private set; }

        public InputAction<SelectableWorkspaceViewModel> SelectWorkspaceAction { get; }

        public SelectDefaultWorkspaceViewModel(
            ITogglDataSource dataSource,
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.dataSource = dataSource;
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;

            SelectWorkspaceAction = InputAction<SelectableWorkspaceViewModel>.FromObservable(selectWorkspace);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Workspaces = await dataSource
                .Workspaces
                .GetAll()
                .Do(throwIfThereAreNoWorkspaces)
                .Select(workspaces => workspaces
                    .Select(toSelectable)
                    .ToImmutableList());
        }

        private SelectableWorkspaceViewModel toSelectable(IThreadSafeWorkspace workspace)
            => new SelectableWorkspaceViewModel(workspace, false);

        private IObservable<Unit> selectWorkspace(SelectableWorkspaceViewModel workspace)
            => Observable.DeferAsync(async _ =>
            {
                await interactorFactory.SetDefaultWorkspace(workspace.WorkspaceId).Execute();
                await navigationService.Close(this);
                return Observable.Return(Unit.Default);
            });

        private void throwIfThereAreNoWorkspaces(IEnumerable<IThreadSafeWorkspace> workspaces)
        {
            if (workspaces.None())
                throw new NoWorkspaceException("Found no local workspaces. This view model should not be used, when there are no workspaces");
        }
    }
}
