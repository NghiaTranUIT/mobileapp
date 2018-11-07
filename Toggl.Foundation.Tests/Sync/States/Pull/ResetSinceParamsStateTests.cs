using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Pull
{
    public sealed class ResetSinceParamsStateTests
    {
        private readonly ISinceParameterRepository sinceParameterRepository = Substitute.For<ISinceParameterRepository>();

        [Theory, LogIfTooSlow]
        [ConstructorData]
        public void ThrowsIfAnyOfTheArgumentsIsNull(bool useSinceParameterRepository)
        {
            var sinceParameterRepository =
                useSinceParameterRepository ? Substitute.For<ISinceParameterRepository>() : null;

            Action tryingToConstructWithNulls = () => new ResetSinceParamsState(sinceParameterRepository);

            tryingToConstructWithNulls.Should().Throw<ArgumentNullException>();
        }

        [Fact, LogIfTooSlow]
        public async Task ResetsSinceParameterRepositoryBeforePersisting()
        {
            var workspaces = new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2 },
            };

            var state = new ResetSinceParamsState(sinceParameterRepository);
            await state.Start(workspaces);

            sinceParameterRepository.Received().Reset();
        }

        [Fact, LogIfTooSlow]
        public async Task ReturnsTheNewWorkspacesUntouched()
        {
            var workspaces = new[]
            {
                new MockWorkspace { Id = 1 },
                new MockWorkspace { Id = 2 },
            };
            var state = new ResetSinceParamsState(sinceParameterRepository);
            var transition = await state.Start(workspaces);
            var parameter = ((Transition<IEnumerable<IWorkspace>>)transition).Parameter;

            parameter.Should().BeSameAs(workspaces);
        }
    }
}
