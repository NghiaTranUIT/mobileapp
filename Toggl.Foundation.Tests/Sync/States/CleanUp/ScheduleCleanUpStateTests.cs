using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States.CleanUp;
using Toggl.Foundation.Tests.Generators;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.CleanUp
{
    public sealed class ScheduleCleanUpStateTests
    {
        private ISyncStateQueue queue = Substitute.For<ISyncStateQueue>();

        private ScheduleCleanUpState state;

        public ScheduleCleanUpStateTests()
        {
            state = new ScheduleCleanUpState(queue);
        }

        [Theory, LogIfTooSlow]
        [ConstructorData]
        public void ThrowsIfAnyOfTheArgumentIsNull(bool useQueue)
        {
            var theQueue = useQueue ? queue : null;
            Action tryingToConstructWithEmptyParameters =
                () => new ScheduleCleanUpState(theQueue);

            tryingToConstructWithEmptyParameters
                .Should().Throw<ArgumentNullException>();
        }

        [Fact, LogIfTooSlow]
        public async Task ShouldQueueTheCleanUp()
        {
            await state.Start().SingleAsync();
            queue.Received().QueueCleanUp();
        }
    }
}
