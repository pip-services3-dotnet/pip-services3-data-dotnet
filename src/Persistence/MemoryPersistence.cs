using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipServices.Commons.Refer;
using PipServices.Commons.Run;
using PipServices.Components.Log;

namespace PipServices.Data.Persistence
{
    /// <summary>
    /// Abstract persistence component that stores data in memory.
    /// 
    /// This is the most basic persistence component that is only
    /// able to store data items of any type.Specific CRUD operations
    /// over the data items must be implemented in child classes by
    /// accessing <c>this._items</c> property and calling <c>Save()</c> method.
    /// 
    /// The component supports loading and saving items from another data source.
    /// That allows to use it as a base class for file and other types
    /// of persistence components that cache all data in memory.
    /// 
    /// ### References ###
    /// 
    /// - *:logger:*:*:1.0         (optional) <a href="https://rawgit.com/pip-services-dotnet/pip-services-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_log_1_1_i_logger.html">ILogger</a> components to pass log messages
    /// </summary>
    /// <typeparam name="T">the class type</typeparam>
    /// <example>
    /// <code>
    /// class MyMemoryPersistence extends MemoryPersistence<MyData>
    /// {
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
    ///     
    /// var persistence = new MyMemoryPersistence();
    /// 
    /// persistence.Set("123", new MyData("ABC"));
    /// Console.Out.WriteLine(persistence.getByName("123", "ABC")).toString(); // Result: { name: "ABC" }
    /// </code>
    /// </example>
    public class MemoryPersistence<T> : IReferenceable, IOpenable, ICleanable
    {
        protected readonly string _typeName;
        protected CompositeLogger _logger = new CompositeLogger();

        protected List<T> _items = new List<T>();
        protected ILoader<T> _loader;
        protected ISaver<T> _saver;
        protected readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        protected bool _opened = false;

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        public MemoryPersistence()
            : this(null, null)
        { }

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        /// <param name="loader">(optional) a loader to load items from external datasource.</param>
        /// <param name="saver">(optional) a saver to save items to external datasource.</param>
        protected MemoryPersistence(ILoader<T> loader, ISaver<T> saver)
        {
            _typeName = typeof(T).Name;
            _loader = loader;
            _saver = saver;
        }

        /// <summary>
        /// Sets references to dependent components.
        /// </summary>
        /// <param name="references">references to locate the component dependencies.</param>
        public void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);
        }

        /// <summary>
        /// Checks if the component is opened.
        /// </summary>
        /// <returns>true if the component has been opened and false otherwise.</returns>
        public bool IsOpen()
        {
            return _opened;
        }

        /// <summary>
        /// Opens the component.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public async Task OpenAsync(string correlationId)
        {
            await LoadAsync(correlationId);
            _opened = true;
        }

        /// <summary>
        /// Closes component and frees used resources.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public async Task CloseAsync(string correlationId)
        {
            await SaveAsync(correlationId);
            _opened = false;
        }

        private Task LoadAsync(string correlationId)
        {
            if (_loader == null)
                return Task.Delay(0);

            _lock.EnterWriteLock();

            try
            {
                _items = _loader.LoadAsync(correlationId).Result;
                _logger.Trace(correlationId, "Loaded {0} of {1}", _items.Count, _typeName);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return Task.Delay(0);
        }

        /// <summary>
        /// Saves items to external data source using configured saver component.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public Task SaveAsync(string correlationId)
        {
            if (_saver == null)
                return Task.Delay(0);

            _lock.EnterWriteLock();

            try
            {
                var task = _saver.SaveAsync(correlationId, _items);
                task.Wait();

                _logger.Trace(correlationId, "Saved {0} of {1}", _items.Count, _typeName);

                return Task.Delay(0);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears component state.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public Task ClearAsync(string correlationId)
        {
            _lock.EnterWriteLock();

            try
            {
                _items = new List<T>();

                _logger.Trace(correlationId, "Cleared {0}", _typeName);

                return SaveAsync(correlationId);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

    }
}