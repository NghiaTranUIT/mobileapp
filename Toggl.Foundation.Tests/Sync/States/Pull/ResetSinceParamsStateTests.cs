using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Pull
{
    public class ResetSinceParamsStateTests
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
            var state = new ResetSinceParamsState(sinceParameterRepository);
            await state.Start(new IWorkspace[] { });

            sinceParameterRepository.Received().Reset();
        }
    }
}
