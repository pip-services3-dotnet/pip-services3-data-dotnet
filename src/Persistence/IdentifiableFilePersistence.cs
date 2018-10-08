using PipServices.Commons.Config;
using PipServices.Commons.Data;

namespace PipServices.Data.Persistence
{
    /// <summary>
    /// Abstract persistence component that stores data in flat files
    /// and implements a number of CRUD operations over data items with unique ids.
    /// The data items must implement IIdentifiable interface.
    /// 
    /// In basic scenarios child classes shall only override <c>GetPageByFilter()</c>,
    /// <c>GetListByFilter()</c> or <c>DeleteByFilter()</c> operations with specific filter function.
    /// All other operations can be used out of the box.
    /// 
    /// In complex scenarios child classes can implement additional operations by
    /// accessing cached items via <c>this._items</c> property and calling <c>Save()</c> method on updates.
    /// 
    /// ### Configuration parameters ###
    /// 
    /// - path:                    path to the file where data is stored
    /// 
    /// options:
    /// - max_page_size:       Maximum number of items returned in a single page (default: 100)
    /// 
    /// ### References ###
    /// 
    /// - *:logger:*:*:1.0         (optional) <a href="https://rawgit.com/pip-services-dotnet/pip-services-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_log_1_1_i_logger.html">ILogger</a> components to pass log messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    /// <example>
    /// <code>
    /// class MyFilePersistence: IdentifiableFilePersistence<MyData, string> 
    /// {
    ///     public MyFilePersistence(string path)
    ///     {
    ///         base(MyData.class, new JsonPersister(path));
    ///         
    ///     private List<Func<MyData, bool>> ComposeFilter(FilterParams filter)
    ///     {
    ///         filter = filter != null ? filter : new FilterParams();
    ///         String name = filter.getAsNullableString("name");
    ///         return List<Func<MyData, bool>>() {
    ///         (item) => {
    ///         if (name != null && item.name != name)
    ///             return false;
    ///         return true;
    ///         }
    ///         };
    ///     }
    ///     
    ///     public DataPage<MyData> GetPageByFilter(string correlationId, FilterParams filter, PagingParams paging)
    ///     {
    ///         base.GetPageByFilter(correlationId, this.composeFilter(filter), paging, null, null);
    ///     }
    /// }
    /// 
    /// var persistence = new MyFilePersistence("./data/data.json");
    /// var item = persistence.Create("123", new MyData("1", "ABC"));
    /// var mydata = persistence.GetPageByFilter(
    /// "123",
    /// FilterParams.fromTuples("name", "ABC"),
    /// null, null, null);
    /// Console.Out.WriteLine(page.Data);          // Result: { id: "1", name: "ABC" }
    /// persistence.DeleteById("123", "1");
    /// ...
    /// </code>
    /// </example>
    /// See <see cref="JsonFilePersister{T}"/>, <see cref="MemoryPersistence{T}"/>
    public class IdentifiableFilePersistence<T, K> : IdentifiableMemoryPersistence<T, K>
        where T : IIdentifiable<K>
        where K : class
    {
        protected readonly JsonFilePersister<T> _persister;

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        /// <param name="persister">(optional) a persister component that loads and saves data from/to flat file.</param>
        public IdentifiableFilePersistence(JsonFilePersister<T> persister)
            : base(persister, persister)
        {
            _persister = persister;
        }

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        public IdentifiableFilePersistence()
            : this(new JsonFilePersister<T>())
        { }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public override void Configure(ConfigParams config)
        {
            base.Configure(config);
            _persister.Configure(config);
        }
    }
}