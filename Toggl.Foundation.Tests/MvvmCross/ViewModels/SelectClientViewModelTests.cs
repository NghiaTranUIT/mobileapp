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

        public sealed class TheSelectClientCommand : SelectClientViewModelTest
        {
            private readonly SelectableClientViewModel client = new SelectableClientViewModel(9, "Client A");

            public TheSelectClientCommand()
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
        }

        public sealed class TheTextProperty : SelectClientViewModelTest
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

        public sealed class TheSuggestCreationProperty : SelectClientViewModelTest
        {
            private const string name = "My client";

            public TheSuggestCreationProperty()
            {
                var client = Substitute.For<IThreadSafeClient>();
                client.Name.Returns(name);
                InteractorFactory.GetAllClientsInWorkspace(Arg.Any<long>())
                    .Execute()
                    .Returns(Observable.Return(new List<IThreadSafeClient> { client }));
                ViewModel.Prepare(Parameters);
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsFalseIfTheTextIsEmpty()
            {
                await ViewModel.Initialize();

//                ViewModel.Text = "";
//
//                ViewModel.SuggestCreation.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsFalseIfTheTextIsOnlyWhitespace()
            {
                await ViewModel.Initialize();

//                ViewModel.Text = "       ";
//
//                ViewModel.SuggestCreation.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsFalseIfTheTextMatchesTheNameOfAnExistingProject()
            {
                await ViewModel.Initialize();

//                ViewModel.Text = name;
//
//                ViewModel.SuggestCreation.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsFalseIfTheTextIsLongerThanTwoHundredAndFiftyCharacters()
            {
                await ViewModel.Initialize();

//                ViewModel.Text = "Some absurdly long project name created solely for making sure that the SuggestCreation property returns false when the project name is longer than the previously specified threshold so that the mobile apps behave and avoid crashes in backend and even bigger problems.";
//
//                ViewModel.SuggestCreation.Should().BeFalse();
            }
        }

        public sealed class TheCreateClientCommand : SelectClientViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task CreatesANewClientWithTheGivenNameInTheCurrentWorkspace()
            {
                long workspaceId = 10;
                await ViewModel.Initialize();
                ViewModel.Prepare(Parameters);
//                ViewModel.Text = "Some name of the client";
//
//                await ViewModel.CreateClientCommand.ExecuteAsync();
//
//                await InteractorFactory
//                    .Received()
//                    .CreateClient(Arg.Is(ViewModel.Text), Arg.Is(workspaceId))
//                    .Execute();
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
                await ViewModel.CreateClient.Execute(name);

                await InteractorFactory
                    .Received()
                    .CreateClient(Arg.Is(trimmed), Arg.Any<long>())
                    .Execute();
            }

            [Theory, LogIfTooSlow]
            [InlineData(" ")]
            [InlineData("\t")]
            [InlineData("\n")]
            [InlineData("               ")]
            [InlineData("      \t  \n     ")]
            public async Task DoesNotSuggestCreatingClientsWhenTheDescriptionConsistsOfOnlyWhiteCharacters(string name)
            {
                await ViewModel.Initialize();

//                ViewModel.Text = name;
//
//                ViewModel.SuggestCreation.Should().BeFalse();
            }

            [Theory, LogIfTooSlow]
            [InlineData(" ")]
            [InlineData("\t")]
            [InlineData("\n")]
            [InlineData("               ")]
            [InlineData("      \t  \n     ")]
            public async Task DoesNotAllowCreatingClientsWhenTheDescriptionConsistsOfOnlyWhiteCharacters(string name)
            {
                await ViewModel.Initialize();
//                ViewModel.Text = name;
//
//                await ViewModel.CreateClientCommand.ExecuteAsync();

                await InteractorFactory.DidNotReceiveWithAnyArgs().CreateClient(null, 0).Execute();
            }
        }
    }
}