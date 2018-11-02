using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;

namespace Toggl.Foundation.Interactors
{
    public sealed class HasFinsihedSyncBeforeInteractor : IInteractor<IObservable<bool>>
    {
        private readonly ITogglDataSource dataSource;

        public HasFinsihedSyncBeforeInteractor(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            this.dataSource = dataSource;
        }

        public IObservable<bool> Execute()
        {
            // We check if Preferences are persisted because
            // only after a successful sync preferences will be stored in the database
            return dataSource.Preferences
                .Get()
                .Select(_ => true)
                .Catch((Exception _) => Observable.Return(false));
        }
    }
}
