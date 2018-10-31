using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Helper.Constants;
using static Toggl.Multivac.Extensions.StringExtensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectClientViewModel : MvxViewModel<SelectClientParameters, long?>
    {
        public ISubject<string> ClientFilterText { get; } = new BehaviorSubject<string>(string.Empty);

        public ObservableGroupedOrderedCollection<SelectableClientViewModel> Clients { get; } =
            new ObservableGroupedOrderedCollection<SelectableClientViewModel>(
                indexKey: c => c.Id,
                orderingKey: c => c.Id,
                groupingKey: _ => "Group"
            );

        public UIAction CloseAction { get; }
        public InputAction<string> CreateClientAction { get; }
        public InputAction<SelectableClientViewModel> SelectClientAction { get; }

        public IObservable<bool> ShowCreationSuggestion { get; }

        public CompositeDisposable DisposeBag = new CompositeDisposable();

        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;
        private readonly ISchedulerProvider schedulerProvider;
        private long workspaceId;
        private IEnumerable<IThreadSafeClient> allClients;
        private ISubject<bool> showCreationSuggestion = new BehaviorSubject<bool>(false);
        private SelectableClientViewModel noClient = new SelectableClientViewModel(0, Resources.NoClient);

        public SelectClientViewModel(
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService,
            ISchedulerProvider schedulerProvider)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;

            CloseAction = UIAction.FromAsync(close);
            CreateClientAction = InputAction<string>.FromAsync(createClient);
            SelectClientAction = InputAction<SelectableClientViewModel>.FromAsync(selectClient);
            ShowCreationSuggestion = showCreationSuggestion.AsDriver(false, schedulerProvider);
        }

        public override void Prepare(SelectClientParameters parameter)
        {
            workspaceId = parameter.WorkspaceId;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            allClients = await interactorFactory.GetAllClientsInWorkspace(workspaceId).Execute();

            ClientFilterText
                .Select(text =>
                {
                    var selectableViewModels = allClients
                        .Where(c => c.Name.ContainsIgnoringCase(text))
                        .Select(toSelectableViewModel);

                    if (string.IsNullOrEmpty(text))
                    {
                        selectableViewModels = selectableViewModels.Prepend(noClient);
                    }
                    return selectableViewModels;
                })
                .Subscribe(clients => Clients.ReplaceWith(clients))
                .DisposedBy(DisposeBag);
        }

        private SelectableClientViewModel toSelectableViewModel(IThreadSafeClient client)
            => new SelectableClientViewModel(client.Id, client.Name);

        private Task close()
            => navigationService.Close(this, null);

        private async Task selectClient(SelectableClientViewModel client)
        {
            await navigationService.Close(this, client.Id);
        }

        private async Task createClient(string clientName)
        {
            var client = await interactorFactory.CreateClient(clientName, workspaceId).Execute();
            await navigationService.Close(this, client.Id);
        }
    }
}
