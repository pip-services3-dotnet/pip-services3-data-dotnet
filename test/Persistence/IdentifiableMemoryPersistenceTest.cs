using System.Threading;
using PipServices3.Commons.Config;
using Xunit;

namespace PipServices3.Data.Persistence
{
    public sealed class IdentifiableMemoryPersistenceTest
    {
        private static IdentifiableMemoryPersistence<PersistenceFixture.Dummy, string> Db 
            = new IdentifiableMemoryPersistence<PersistenceFixture.Dummy, string>();
        private static PersistenceFixture Fixture { get; set; }

        private PersistenceFixture GetFixture()
        {
            return new PersistenceFixture(Db, Db, Db, Db, Db, Db, Db, Db);
        }

        public IdentifiableMemoryPersistenceTest()
        {
            if (Db == null)
                return;

            Db.Configure(new ConfigParams());

            var task = Db.OpenAsync(null);
            task.Wait();

            task = Db.ClearAsync(null);
            task.Wait();

            Fixture = GetFixture();
        }

        [Fact]
        public void TestCrudOperations()
        {
            if (Fixture == null) return;

            var task = Fixture.TestCrudOperationsAsync();
            task.Wait();
        }

        [Fact]
        public void TestMultithreading()
        {
            if (Fixture == null) return;

            var task = Fixture.TestMultithreading();
            task.Wait();
        }
    }
}
