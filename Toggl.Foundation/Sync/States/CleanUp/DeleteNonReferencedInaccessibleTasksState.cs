using System;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States.CleanUp
{
    public sealed class DeleteNonReferencedInaccessibleTasksState : DeleteInaccessibleEntityState<IThreadSafeTask, IDatabaseTask>
    {
        private IDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry> timeEntriesDataSource;

        public DeleteNonReferencedInaccessibleTasksState(
            IDataSource<IThreadSafeTask, IDatabaseTask> tasksDataSource,
            IDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry> timeEntriesDataSource
            ) : base(tasksDataSource)
        {
            Ensure.Argument.IsNotNull(timeEntriesDataSource, nameof(timeEntriesDataSource));
            this.timeEntriesDataSource = timeEntriesDataSource;
        }

        protected override IObservable<bool> SuitableForDeletion(IThreadSafeTask task)
            => timeEntriesDataSource.GetAll(
                    timeEntry => isReferenced(task, timeEntry),
                    includeInaccessibleEntities: true)
                .Select(references => references.None());

        private bool isReferenced(ITask task, ITimeEntry timeEntry)
            => timeEntry.TaskId == task.Id;
    }
}
