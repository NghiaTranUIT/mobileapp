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
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Pull
{
    public sealed class MarkWorkspacesAsInaccessibleStateTests
    {
        public sealed class TheConstructor
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource)
            {
                Action tryingToConstructWithNulls = () => new MarkWorkspacesAsInaccessibleState(
                    useDataSource ? Substitute.For<IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace>>() : null
                );

                tryingToConstructWithNulls.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheStartMethod
        {
            private readonly IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace> dataSource =
                Substitute.For<IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace>>();

            [Fact]
            public async Task ReturnsFetchObservablesAsParameterOfTransition()
            {
                prepareDatabase(new[]
                {
                    new MockWorkspace { Id = 1 },
                    new MockWorkspace { Id = 2 },
                    new MockWorkspace { Id = 3 }
                });

                var inaccessibleWorkspaces = new[]
                {
                    new MockWorkspace { Id = 1 },
                };

                var fetchObservables = Substitute.For<IFetchObservables>();

                var state = new MarkWorkspacesAsInaccessibleState(dataSource);
                var stateParams = new MarkWorkspacesAsInaccessibleParams(inaccessibleWorkspaces, fetchObservables);

                var transition = await state.Start(stateParams);
                var parameter = ((Transition<IFetchObservables>)transition).Parameter;

                parameter.Should().Be(fetchObservables);
            }

            [Fact, LogIfTooSlow]
            public async Task MarksWorkspacesAsInaccessible()
            {
                prepareDatabase(new[]
                {
                    new MockWorkspace { Id = 1 },
                    new MockWorkspace { Id = 2 },
                    new MockWorkspace { Id = 3 }
                });

                var inaccessibleWorkspaces = new[]
                {
                    new MockWorkspace { Id = 1 }
                };

                var fetchObservables = Substitute.For<IFetchObservables>();

                var state = new MarkWorkspacesAsInaccessibleState(dataSource);
                var stateParams = new MarkWorkspacesAsInaccessibleParams(inaccessibleWorkspaces, fetchObservables);

                await state.Start(stateParams);

                await dataSource.Received()
                    .Update(Arg.Is<IThreadSafeWorkspace>(workspace => workspace.Id == 1 && workspace.IsInaccessible));
            }

            private void prepareDatabase(IEnumerable<IThreadSafeWorkspace> workspaces)
            {
                dataSource.GetAll(Arg.Any<Func<IDatabaseWorkspace, bool>>())
                    .Returns(callInfo =>
                        Observable.Return(
                            workspaces.Where<IThreadSafeWorkspace>(callInfo.Arg<Func<IDatabaseWorkspace, bool>>())));
            }
        }
    }
}
