using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SelectClientViewModelTests
    {
        public abstract class SelectClientViewModelTest : BaseViewModelTests<SelectClientViewModel>
        {
            protected SelectClientParameters Parameters { get; }
                = SelectClientParameters.WithIds(10);

            protected override SelectClientViewModel CreateViewModel()
               => new SelectClientViewModel(InteractorFactory, NavigationService, SchedulerProvider);

            protected List<IThreadSafeClient> GenerateClientList() =>
                Enumerable.Range(-5, 5).Select(i =>
                {
                    var client = Substitute.For<IThreadSafeClient>();
                    client.Id.Returns(i);
                    client.Name.Returns(i.ToString());
                    return client;
                }).ToList();
        }

        public sealed class TheConstructor : SelectClientViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useInteractorFactory,
                bool useNavigationService,
                bool useSchedulerProvider)
            {
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var schedulerProvider = useSchedulerProvider ? SchedulerProvider : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new SelectClientViewModel(interactorFactory, navigationService, schedulerProvider);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheInitializeMethod : SelectClientViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task AddsAllClientsToTheListOfSuggestions()
            {
                var clients = GenerateClientList();
                InteractorFactory.GetAllClientsInWorkspace(Arg.Any<long>())
                    .Execute()
                    .Returns(Observable.Return(clients));
                ViewModel.Prepare(Parameters);

                await ViewModel.Initialize();

                ViewModel.Clients.TotalCount.Should().Equals(clients.Count);
            }

            [Fact, LogIfTooSlow]
            public async Task AddsANoClientSuggestion()
            {
                var clients = GenerateClientList();
                InteractorFactory.GetAllClientsInWorkspace(Arg.Any<long>())
                    .Execute()
                    .Returns(Observable.Return(clients));
                ViewModel.Prepare(Parameters);

                await ViewModel.Initialize();

                ViewModel.Clients.First().First().Name.Should().Be(Resources.NoClient);
            }
        }

        public sealed class TheCloseAction : SelectClientViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.Initialize();

                await ViewModel.Close.Execute();

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), Arg.Any<long?>());
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsNull()
            {
                await ViewModel.Initialize();

                await ViewModel.Close.Execute();

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), null);
            }
        }

        public sealed class TheSelectClientAction : SelectClientViewModelTest
        {
            private readonly SelectableClientViewModel client = new SelectableClientViewModel(9, "Client A", false);

            public TheSelectClientAction()
            {
                var clients = GenerateClientList();
                InteractorFactory.GetAllClientsInWorkspace(Arg.Any<long>())
                    .Execute()
                    .Returns(Observable.Return(clients));
                ViewModel.Prepare(Parameters);
            }

            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.Initialize();

                ViewModel.SelectClient.Execute(client);

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), Arg.Any<long?>());
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsTheSelectedClientId()
            {
                await ViewModel.Initialize();

                ViewModel.SelectClient.Execute(client);

                await NavigationService.Received().Close(
                    Arg.Is(ViewModel),
                    Arg.Is<long?>(client.Id)
                );
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesANewClientWithTheGivenNameInTheCurrentWorkspace()
            {
                long workspaceId = 10;
                await ViewModel.Initialize();
                var newClient = new SelectableClientViewModel(long.MinValue, "Some name of the client", true);

                await ViewModel.SelectClient.Execute(newClient);

                await InteractorFactory
                    .Received()
                    .CreateClient(Arg.Is(newClient.Name), Arg.Is(workspaceId))
                    .Execute();
            }

            [Theory, LogIfTooSlow]
            [InlineData("   abcde", "abcde")]
            [InlineData("abcde     ", "abcde")]
            [InlineData("  abcde ", "abcde")]
            [InlineData("abcde  fgh", "abcde  fgh")]
            [InlineData("      abcd\nefgh     ", "abcd\nefgh")]
            public async Task TrimsNameFromTheStartAndTheEndBeforeSaving(string name, string trimmed)
            {
                await ViewModel.Initialize();
                await ViewModel.SelectClient.Execute(new SelectableClientViewModel(long.MinValue, name, true));

                await InteractorFactory
                    .Received()
                    .CreateClient(Arg.Is(trimmed), Arg.Any<long>())
                    .Execute();
            }

        }

        public sealed class TheFilterTextInput : SelectClientViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task FiltersTheSuggestionsWhenItChanges()
            {
                var clients = GenerateClientList();
                InteractorFactory.GetAllClientsInWorkspace(Arg.Any<long>())
                    .Execute()
                    .Returns(Observable.Return(clients));
                ViewModel.Prepare(Parameters);
                await ViewModel.Initialize();

                ViewModel.ClientFilterText.OnNext("0");
                ViewModel.Clients.TotalCount.Should().Equals(1);
            }
        }
    }
}