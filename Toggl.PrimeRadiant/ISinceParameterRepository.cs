using System;
using System.Collections.Generic;

namespace Toggl.PrimeRadiant
{
    public interface ISinceParameterRepository
    {
        DateTimeOffset? Get<T>() where T : IDatabaseSyncable;

        void Set<T>(DateTimeOffset? since);

        bool Supports<T>();

        void Reset();

        IDictionary<string, DateTimeOffset?> GetAll();
    }
}
