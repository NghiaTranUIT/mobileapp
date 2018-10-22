using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Realm;

namespace Toggl.Foundation.Sync.Tests
{
    public sealed class Storage
    {
        public ITogglDatabase Database { get; }

        public Storage()
        {
            Database = new Database();
        }

        public DatabaseState Load()
        {
            return default(DatabaseState);
        }

        public void Store(DatabaseState databaseState)
        {
        }
    }
}
