using System;

namespace Toggl.Foundation.Sync.States.Pull
{
    public sealed class EnsureFetchSingletonSucceededState<T> : EnsureFetchSucceededState<T>
    {
        protected override IObservable<T> FetchObservable(IFetchObservables fetch)
            => fetch.GetSingle<T>();
    }
}
