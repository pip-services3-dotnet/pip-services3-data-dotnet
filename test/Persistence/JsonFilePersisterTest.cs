using PipServices3.Commons.Errors;
using PipServices3.Commons.Config;
using Xunit;

namespace PipServices3.Data.Persistence
{
    public sealed class JsonFilePersisterTest
    {
        private readonly JsonFilePersister<PersistenceFixture.Dummy> _persister;

        public JsonFilePersisterTest()
        {
            _persister = new JsonFilePersister<PersistenceFixture.Dummy>();
        }

        [Fact]
        public void Configure_IfNoPathKey_Fails()
        {
            Assert.Throws<ConfigException>(() => _persister.Configure(new ConfigParams()));
        }

        [Fact]
        public void Configure_IfPathKeyCheckProperty_IsOk()
        {
            const string fileName = nameof(JsonFilePersisterTest);

            _persister.Configure(ConfigParams.FromTuples("path", fileName));

            Assert.Equal(fileName, _persister.Path);
        }

        public void LoadAsync_()
        {

        }

        public void SaveAsync_()
        {

        }
    }
}
