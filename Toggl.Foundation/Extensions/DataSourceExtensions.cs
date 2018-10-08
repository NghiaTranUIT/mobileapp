using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Extensions
{
    public static class DataSourceExtensions
    {
        public static IObservable<Unit> ItemsChanged<TThreadsafe, TDatabase>(this IObservableDataSource<TThreadsafe, TDatabase> dataSource)
            where TDatabase : IDatabaseSyncable
            where TThreadsafe : IThreadSafeModel, TDatabase
            => Observable.Merge(
                dataSource.Created.SelectUnit(),
                dataSource.Updated.SelectUnit(),
                dataSource.Deleted.SelectUnit()
            );
    }
}
