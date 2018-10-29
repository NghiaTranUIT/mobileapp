using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Pull
{
    public class PersistNewWorkspacesStateTests
    {
        private readonly IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace> dataSource =
            Substitute.For<IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace>>();

        private readonly ISinceParameterRepository sinceParameterRepository = Substitute.For<ISinceParameterRepository>();

        [Theory, LogIfTooSlow]
        [ConstructorData]
        public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource, bool useSinceParameterRepository)
        {
            var dataSource = useDataSource
                ? Substitute.For<IDataSource<IThreadSafeWorkspace, IDatabaseWorkspace>>()
                : null;
            var sinceParameterRepository =
                useSinceParameterRepository ? Substitute.For<ISinceParameterRepository>() : null;

            Action tryingToConstructWithNulls = () => new PersistNewWorkspacesState(dataSource, sinceParameterRepository);

            tryingToConstructWithNulls.Should().Throw<ArgumentNullException>();
        }

        [Fact, LogIfTooSlow]
        public async Task PersistsNewWorkspaces()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
            });

            var newWorkspaces = new[]
            {
                new MockWorkspace { Id = 2 },
                new MockWorkspace { Id = 3 },
            };

            var state = new PersistNewWorkspacesState(dataSource, sinceParameterRepository);
            await state.Start(newWorkspaces);

            dataSource.Received(2).Create(Arg.Any<IThreadSafeWorkspace>());
            dataSource.Received().Create(Arg.Is<IThreadSafeWorkspace>(workspace => workspace.Id == 2));
            dataSource.Received().Create(Arg.Is<IThreadSafeWorkspace>(workspace => workspace.Id == 3));
        }

        [Fact, LogIfTooSlow]
        public async Task ReplacesOldInaccessibleWorkspaces()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2, IsInaccessible = true },
            });

            var newWorkspaces = new[]
            {
                new MockWorkspace { Id = 2 }
            };

            var state = new PersistNewWorkspacesState(dataSource, sinceParameterRepository);
            await state.Start(newWorkspaces);

            dataSource.Received().Update(Arg.Is<IThreadSafeWorkspace>(arg => arg.Id == 2));
        }

        [Fact, LogIfTooSlow]
        public async Task CreatesNewWorkspacesAndUpdatesOldInaccessibleWorkspaces()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2, IsInaccessible = true },
            });

            var newWorkspaces = new[]
            {
                new MockWorkspace { Id = 2 },
                new MockWorkspace { Id = 3 },
                new MockWorkspace { Id = 4 },
            };

            var state = new PersistNewWorkspacesState(dataSource, sinceParameterRepository);
            await state.Start(newWorkspaces);

            dataSource.Received().Update(Arg.Is<IThreadSafeWorkspace>(workspace => workspace.Id == 2));

            dataSource.Received(2).Create(Arg.Any<IThreadSafeWorkspace>());
            dataSource.Received().Create(Arg.Is<IThreadSafeWorkspace>(workspace => workspace.Id == 3));
            dataSource.Received().Create(Arg.Is<IThreadSafeWorkspace>(workspace => workspace.Id == 4));
        }

        [Fact, LogIfTooSlow]
        public async Task PersistedNewWorkspacesAreNotMarkedAsInaccessible()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2, IsInaccessible = true },
            });

            var newWorkspaces = new[]
            {
                new MockWorkspace { Id = 2 },
                new MockWorkspace { Id = 3 }
            };

            var state = new PersistNewWorkspacesState(dataSource, sinceParameterRepository);
            await state.Start(newWorkspaces);

            dataSource.Received().Update(Arg.Is<IThreadSafeWorkspace>(workspace => !workspace.IsInaccessible));
            dataSource.Received().Create(Arg.Is<IThreadSafeWorkspace>(workspace => !workspace.IsInaccessible));
        }

        [Fact, LogIfTooSlow]
        public async Task ResetsSinceParameterRepositoryBeforePersisting()
        {
            prepareDatabase(new[]
            {
                new MockWorkspace { Id = 1 },
            });

            var newWorkspaces = new[]
            {
                new MockWorkspace { Id = 2 }
            };

            var state = new PersistNewWorkspacesState(dataSource, sinceParameterRepository);
            await state.Start(newWorkspaces);

            Received.InOrder(() =>
            {
                sinceParameterRepository.Reset();
                dataSource.Create(Arg.Any<IThreadSafeWorkspace>());
            });
        }

        private void prepareDatabase(IEnumerable<IThreadSafeWorkspace> workspaces)
        {
            dataSource.GetAll(Arg.Any<Func<IDatabaseWorkspace, bool>>(), Arg.Is(true)).Returns(callInfo =>
                {
                    var predicate = callInfo[0] as Func<IDatabaseWorkspace, bool>;
                    var filteredWorkspaces = workspaces.Where(predicate);
                    return Observable.Return(filteredWorkspaces.Cast<IThreadSafeWorkspace>());
                });
        }
    }
}
