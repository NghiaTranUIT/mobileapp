using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Helper.Constants;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectClientViewModel : MvxViewModel<SelectClientParameters, long?>
    {
        public ISubject<string> ClientFilterText { get; } = new BehaviorSubject<string>(string.Empty);

        public IObservable<IEnumerable<SelectableClientViewModel>> Clients { get; private set; }

        public UIAction Close { get; }

        public InputAction<SelectableClientViewModel> SelectClient { get; }

        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;
        private readonly ISchedulerProvider schedulerProvider;
        private long workspaceId;
        private IEnumerable<IThreadSafeClient> allClients;
        private SelectableClientViewModel noClient = new SelectableClientViewModel(0, Resources.NoClient, false);

        public SelectClientViewModel(
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService,
            ISchedulerProvider schedulerProvider)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;

            Close = UIAction.FromAsync(close);
            SelectClient = InputAction<SelectableClientViewModel>.FromAsync(selectClient);
        }

        public override void Prepare(SelectClientParameters parameter)
        {
            workspaceId = parameter.WorkspaceId;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            allClients = await interactorFactory.GetAllClientsInWorkspace(workspaceId).Execute();

            Clients = ClientFilterText
                .Select(text =>
                {
                    var trimmedText = text.Trim();
                    var selectableViewModels = allClients
                        .Where(c => c.Name.ContainsIgnoringCase(trimmedText))
                        .Select(toSelectableViewModel);

                    var suggestCreation = allClients.None(c => c.Name == trimmedText)
                                          && trimmedText.LengthInBytes() <= MaxClientNameLengthInBytes;

                    if (string.IsNullOrEmpty(trimmedText))
                    {
                        selectableViewModels = selectableViewModels.Prepend(noClient);
                    }
                    else if (suggestCreation)
                    {
                        var creationSelectableViewModel =
                            new SelectableClientViewModel(long.MinValue, trimmedText, true);
                        selectableViewModels = selectableViewModels.Prepend(creationSelectableViewModel);
                    }

                    return selectableViewModels;
                });
        }

        private SelectableClientViewModel toSelectableViewModel(IThreadSafeClient client)
            => new SelectableClientViewModel(client.Id, client.Name, false);

        private Task close()
            => navigationService.Close(this, null);

        private async Task selectClient(SelectableClientViewModel client)
        {
            if (client.IsCreation)
            {
                var newClient = await interactorFactory.CreateClient(client.Name.Trim(), workspaceId).Execute();
                await navigationService.Close(this, newClient.Id);
            }
            else
            {
                await navigationService.Close(this, client.Id);
            }
        }
    }
}
