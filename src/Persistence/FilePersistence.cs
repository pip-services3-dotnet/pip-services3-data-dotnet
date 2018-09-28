using PipServices.Commons.Config;

namespace PipServices.Data.Persistence
{
    /// <summary>
    /// Abstract persistence component that stores data in flat files
    /// and caches them in memory.
    /// 
    /// This is the most basic persistence component that is only
    /// able to store data items of any type.Specific CRUD operations 
    /// over the data items must be implemented in child classes by
    /// accessing this._items property and calling Save() method.
    /// 
    /// ### Configuration parameters ###
    /// 
    /// path:                path to the file where data is stored
    /// 
    /// ### References ###
    /// 
    /// - *:logger:*:*:1.0   (optional) ILogger components to pass log messages
    /// </summary>
    /// <typeparam name="T">the class type</typeparam>
    /// <example>
    /// <code>
    /// class MyJsonFilePersistence: FilePersistence<MyData> 
    /// {
    ///     public MyJsonFilePersistence(string path)
    ///     {
    ///         super(MyData.class, new JsonFilePersister(path));
    ///     }
    ///     public MyData GetByName(String correlationId, String name)
    ///     {
    ///         MyData item = _items.Find((mydata) => { return mydata.Name == name; });
    ///         ...
    ///         return item;
    ///     } 
    ///     public MyData Set(String correlatonId, MyData item)
    ///     {
    ///         this._items = _items.Filter((mydata) => { return mydata.Name != name; });
    ///         ...
    ///         this._items.add(item);
    ///         this.save(correlationId);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// See <see cref="MemoryPersistence{T}"/>, <see cref="JsonFilePersister{T}"/>
    public class FilePersistence<T> : MemoryPersistence<T>
    {
        protected readonly JsonFilePersister<T> _persister;

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        /// <param name="persister">(optional) a persister component that loads and saves data from/to flat file.</param>
        public FilePersistence(JsonFilePersister<T> persister)
            : base(persister, persister)
        {
            _persister = persister;
        }

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        public FilePersistence()
            : this(new JsonFilePersister<T>())
        { }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public virtual void Configure(ConfigParams config)
        {
            _persister.Configure(config);
        }
    }
}