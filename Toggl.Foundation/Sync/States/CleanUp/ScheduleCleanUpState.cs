using System;
using System.Reactive.Linq;
using Toggl.Multivac;

namespace Toggl.Foundation.Sync.States.CleanUp
{
    internal sealed class ScheduleCleanUpState : ISyncState
    {
        private readonly ISyncStateQueue queue;

        public StateResult CleanUpScheduled { get; } = new StateResult();

        public ScheduleCleanUpState(ISyncStateQueue queue)
        {
            Ensure.Argument.IsNotNull(queue, nameof(queue));
            this.queue = queue;
        }

        public IObservable<ITransition> Start()
        {
            queue.QueueCleanUp();
            return Observable.Return(CleanUpScheduled.Transition());
        }
    }
}
