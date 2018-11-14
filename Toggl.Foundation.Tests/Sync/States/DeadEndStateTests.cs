using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Foundation.Sync.States;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States
{
    public class DeadEndStateTests
    {
        public class TheStartMethod
        {
            [Fact, LogIfTooSlow]
            public void DoesNotThrowWhenNobodySubscribesToTheReturnedObservable()
            {
                var state = new DeadEndState();

                state.Start();
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsASingleTransitionWithAStateResult()
            {
                var state = new DeadEndState();

                var transition = await state.Start().SingleAsync();

                transition.Should().NotBeNull();
                transition.Result.Should().NotBeNull();
            }
        }
    }
}
