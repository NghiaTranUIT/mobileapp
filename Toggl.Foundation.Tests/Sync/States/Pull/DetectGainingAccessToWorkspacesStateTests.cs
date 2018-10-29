using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
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
    public sealed class DetectGainingAccessToWorkspacesStateTests
    {
        private readonly IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace> dataSource =
            Substitute.For<IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace>>();

        private readonly IFetchObservables fetchObservables = Substitute.For<IFetchObservables>();

        [Theory, LogIfTooSlow]
        [ConstructorData]
        public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource)
        {
            Action tryingToConstructAction = () => new DetectGainingAccessToWorkspacesState(
                useDataSource ? Substitute.For<IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace>>() : null
            );

            tryingToConstructAction.Should().Throw<ArgumentNullException>();
        }

        [Fact, LogIfTooSlow]
        public async Task ReturnsFetchObservablesIfNoNewWorkspaceIsDetected()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2 }
            });
            prepareFetch(new List<IWorkspace>
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2 }
            });
            var state = new DetectGainingAccessToWorkspacesState(dataSource);

            var transition = await state.Start(fetchObservables);
            var parameter = ((Transition<IFetchObservables>)transition).Parameter;

            parameter.Should().Be(fetchObservables);
        }

        [Fact, LogIfTooSlow]
        public async Task ReturnsNewWorkspacesDetectedIfNewWorkspaceAreDetected()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2 }
            });
            prepareFetch(new List<IWorkspace>
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2 },
                new MockWorkspace { Id = 3 },
            });
            var state = new DetectGainingAccessToWorkspacesState(dataSource);

            var transition = await state.Start(fetchObservables);
            var parameter = ((Transition<IEnumerable<IWorkspace>>)transition).Parameter;

            parameter.Should().Match(newWorkspaces => newWorkspaces.All(ws => ws.Id == 3));
        }

        [Fact, LogIfTooSlow]
        public async Task ReturnsNewWorkspacesDetectedIfStoredWorkspaceIsInaccessibleAndHasSameId()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2, IsInaccessible = true }
            });
            prepareFetch(new List<IWorkspace>
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2 },
            });
            var state = new DetectGainingAccessToWorkspacesState(dataSource);

            var transition = await state.Start(fetchObservables);
            var parameter = ((Transition<IEnumerable<IWorkspace>>)transition).Parameter;

            parameter.Should().Match(newWorkspaces => newWorkspaces.All(ws => ws.Id == 2));
        }

        [Fact, LogIfTooSlow]
        public async Task ReturnsNewWorkspacesDetectedIfNewWorkspaceAreDetectedEvenIfSomeAccessWasLostInAnotherWorkspace()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2 }
            });
            prepareFetch(new List<IWorkspace>
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 3 },
            });
            var state = new DetectGainingAccessToWorkspacesState(dataSource);

            var transition = await state.Start(fetchObservables);
            var parameter = ((Transition<IEnumerable<IWorkspace>>)transition).Parameter;

            parameter.Should().Match(newWorkspaces => newWorkspaces.All(ws => ws.Id == 3));
        }

        private void prepareDatabase(IEnumerable<IThreadSafeWorkspace> workspaces)
        {
            var accessibleWorkspaces = workspaces.Where(ws => !ws.IsInaccessible);
            dataSource.GetAll().Returns(Observable.Return(accessibleWorkspaces));
        }

        private void prepareFetch(List<IWorkspace> workspaces)
        {
            fetchObservables.GetList<IWorkspace>().Returns(Observable.Return(workspaces));
        }
    }
}
