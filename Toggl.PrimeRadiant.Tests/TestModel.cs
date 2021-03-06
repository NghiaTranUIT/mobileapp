using Toggl.Multivac.Models;

namespace Toggl.PrimeRadiant.Tests
{
    public interface ITestModel : IIdentifiable, IDatabaseSyncable { }

    public sealed class TestModel : ITestModel
    {
        public long Id { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }
    }
}