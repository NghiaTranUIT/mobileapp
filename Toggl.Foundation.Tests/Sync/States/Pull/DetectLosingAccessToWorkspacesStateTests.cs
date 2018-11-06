using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Pull
{
    public sealed class DetectLosingAccessToWorkspacesStateTests
    {

        public sealed class TheConstructor
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useDataSource, bool useAnalyticsService)
            {
                Action tryingToConstructWithNulls = () => new DetectLosingAccessToWorkspacesState(
                    useDataSource ? Substitute.For<IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace>>() : null,
                    useAnalyticsService ? Substitute.For<IAnalyticsService>() : null
                );

                tryingToConstructWithNulls.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheStartMethod
        {
            private readonly IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace> dataSource =
                Substitute.For<IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace>>();

            private IAnalyticsService analyticsService { get; } = Substitute.For<IAnalyticsService>();

            private readonly IFetchObservables fetchObservables = Substitute.For<IFetchObservables>();

            [Fact]
            public async Task ReturnsFetchObservablesWhenNoAccessWasLost()
            {
                prepareDatabase(new[]
                {
                    new MockWorkspace { Id = 1 },
                    new MockWorkspace { Id = 2 },
                    new MockWorkspace { Id = 3 }
                });
                prepareFetch(new List<IWorkspace>
                {
                    new MockWorkspace { Id = 1 },
                    new MockWorkspace { Id = 2 },
                    new MockWorkspace { Id = 3 }
                });
                var state = new DetectLosingAccessToWorkspacesState(dataSource, analyticsService);

                var transition = await state.Start(fetchObservables);
                var parameter = ((Transition<IFetchObservables>)transition).Parameter;

                parameter.Should().Be(fetchObservables);
            }

            [Fact]
            public async Task ReturnsWorkspacesAndFetchObservableWhenAccessToSomeWorkspaceWasLost()
            {
                prepareDatabase(new[]
                {
                    new MockWorkspace { Id = 1 },
                    new MockWorkspace { Id = 2 },
                    new MockWorkspace { Id = 3 }
                });
                prepareFetch(new List<IWorkspace>
                {
                    new MockWorkspace { Id = 1 },
                    new MockWorkspace { Id = 2 },
                });
                var state = new DetectLosingAccessToWorkspacesState(dataSource, analyticsService);

                var transition = await state.Start(fetchObservables);
                var parameter = ((Transition<MarkWorkspacesAsInaccessibleParams>)transition).Parameter;

                parameter.Workspaces.Should().OnlyContain(workspace => workspace.Id == 3);
                parameter.FetchObservables.Should().Be(fetchObservables);
            }

            [Fact]
            public async Task IgnoresWorkspacesWhichAreStoredOnlyLocally()
            {
                prepareDatabase(new[]
                {
                    new MockWorkspace { Id = 1 },
                    new MockWorkspace { Id = 2 },
                    new MockWorkspace { Id = -3 }
                });
                prepareFetch(new List<IWorkspace>
                {
                    new MockWorkspace { Id = 1 }
                });
                var state = new DetectLosingAccessToWorkspacesState(dataSource, analyticsService);

                var transition = await state.Start(fetchObservables);
                var parameter = ((Transition<MarkWorkspacesAsInaccessibleParams>)transition).Parameter;
                parameter.Workspaces.Should().OnlyContain(workspace => workspace.Id == 2);
            }

            [Fact]
            public async Task IgnoresInaccessibleWorkspaces()
            {
                prepareDatabase(new[]
                {
                    new MockWorkspace { Id = 1 },
                    new MockWorkspace { Id = 2 },
                    new MockWorkspace { Id = 3, IsInaccessible = true }
                });
                prepareFetch(new List<IWorkspace>
                {
                    new MockWorkspace { Id = 1 }
                });
                var state = new DetectLosingAccessToWorkspacesState(dataSource, analyticsService);

                var transition = await state.Start(fetchObservables);
                var parameter = ((Transition<MarkWorkspacesAsInaccessibleParams>)transition).Parameter;

                parameter.Workspaces.Should().OnlyContain(workspace => workspace.Id == 2);
            }

            private void prepareDatabase(IEnumerable<IThreadSafeWorkspace> workspaces)
            {
                dataSource.GetAll(Arg.Any<Func<IDatabaseWorkspace, bool>>())
                    .Returns(callInfo =>
                        Observable.Return(
                            workspaces.Where<IThreadSafeWorkspace>(callInfo.Arg<Func<IDatabaseWorkspace, bool>>())));
            }

            private void prepareFetch(List<IWorkspace> workspaces)
            {
                fetchObservables.GetList<IWorkspace>().Returns(Observable.Return(workspaces));
            }
        }
    }
}
