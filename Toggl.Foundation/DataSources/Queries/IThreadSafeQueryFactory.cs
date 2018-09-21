using System.Collections.Generic;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant.Queries;

namespace Toggl.Foundation.DataSources.Queries
{
    public interface IThreadSafeQueryFactory
    {
        IQuery<IThreadSafeProject> GetAllProjectsContaining(IEnumerable<string> words);
    }
}