using PipServices.Commons.Config;

namespace PipServices.Data.Persistence
{
    public class FilePersistence<T> : MemoryPersistence<T>
    {
        protected readonly JsonFilePersister<T> _persister;

        public FilePersistence(JsonFilePersister<T> persister)
            : base(persister, persister)
        {
            _persister = persister;
        }

        public FilePersistence()
            : this(new JsonFilePersister<T>())
        { }

        public virtual void Configure(ConfigParams config)
        {
            _persister.Configure(config);
        }
    }
}