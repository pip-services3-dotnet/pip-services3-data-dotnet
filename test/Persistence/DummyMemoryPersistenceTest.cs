using PipServices3.Commons.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PipServices3.Data.Persistence
{
    public class DummyMemoryPersistenceTest : IDisposable
    {
        private DummyMemoryPersistence persistence;
        private PersistenceFixture2 fixture;

        public DummyMemoryPersistenceTest()
        {
            persistence = new DummyMemoryPersistence();
            persistence.Configure(new ConfigParams());

            fixture = new PersistenceFixture2(persistence);

            persistence.OpenAsync(null).Wait();
        }

        public void Dispose()
        {
            persistence.CloseAsync(null).Wait();
        }

        [Fact]
        public async Task TestCrudOperationsAsync()
        {
            await fixture.TestCrudOperationsAsync();
        }

        //[Fact]
        //public async Task TestGetWithFiltersAsync()
        //{
        //    await fixture.TestGetWithFiltersAsync();
        //}
    }
}
