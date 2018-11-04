using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipServices3.Commons.Config;
using PipServices3.Commons.Data;
using PipServices3.Commons.Reflect;

namespace PipServices3.Data.Persistence
{
    /// <summary>
    /// Abstract persistence component that stores data in memory
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
    /// options:
    /// - max_page_size:       Maximum number of items returned in a single page (default: 100)
    /// 
    /// ### References ###
    /// 
    /// - *:logger:*:*:1.0         (optional) <a href="https://rawgit.com/pip-services3-dotnet/pip-services3-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_log_1_1_i_logger.html">ILogger</a> components to pass log messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    /// <example>
    /// <code>
    /// class MyMemoryPersistence: IdentifiableMemoryPersistence<MyData, string> 
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
    /// var persistence = new MyMemoryPersistence("./data/data.json");
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
    public class IdentifiableMemoryPersistence<T, K> : MemoryPersistence<T>, IReconfigurable,
        IWriter<T, K>, IGetter<T, K>, ISetter<T>
        where T : IIdentifiable<K>
        where K : class
    {
        protected int _maxPageSize = 100;

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        public IdentifiableMemoryPersistence()
            : this(null, null)
        { }

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        /// <param name="loader">(optional) a loader to load items from external datasource.</param>
        /// <param name="saver">(optional) a saver to save items to external datasource.</param>
        protected IdentifiableMemoryPersistence(ILoader<T> loader, ISaver<T> saver)
            : base(loader, saver)
        { }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public virtual void Configure(ConfigParams config)
        {
            // Todo: Use connection and auth components
            _maxPageSize = config.GetAsIntegerWithDefault("max_page_size", _maxPageSize);
        }

        private List<T> Filter(IList<T> items, IList<Func<T, bool>> matchFunctions)
        {
            var result = new List<T>();

            foreach (var item in items)
            {
                var isMatched = true;

                foreach (var matchFunction in matchFunctions)
                {
                    isMatched &= matchFunction(item);
                }

                if (isMatched)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a page of data items retrieved by a given filter and sorted according to sort parameters.
        /// 
        /// This method shall be called by a public getPageByFilter method from child class that
        /// receives FilterParams and converts them into a filter function.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="matchFunctions">(optional) a filter function to filter items</param>
        /// <param name="paging">(optional) paging parameters</param>
        /// <returns>a data page of result by filter.</returns>
        public async Task<DataPage<T>> GetPageByFilterAsync(string correlationId,
            IList<Func<T, bool>> matchFunctions, PagingParams paging)
        {
            _lock.EnterReadLock();

            try
            {
                var filteredItems = Filter(_items, matchFunctions);

                paging = paging ?? new PagingParams();
                var skip = paging.GetSkip(0);
                var take = paging.GetTake(_maxPageSize);

                _logger.Trace(correlationId, $"Retrieved {filteredItems.Count} items");

                return await Task.FromResult(new DataPage<T>()
                {
                    Data = filteredItems.Take((int)take).Skip((int)skip).ToList(),
                    Total = paging.Total ? filteredItems.Count : (long?)null
                });
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets a list of data items retrieved by a given filter and sorted according to sort parameters.
        /// 
        /// This method shall be called by a public getListByFilter method from child
        /// class that receives FilterParams and converts them into a filter function.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="matchFunctions">(optional) a filter function to filter items</param>
        /// <returns>a data list of results by filter.</returns>
        public async Task<List<T>> GetListByFilterAsync(string correlationId,
            IList<Func<T, bool>> matchFunctions)
        {
            _lock.EnterReadLock();

            try
            {
                var filteredItems = Filter(_items, matchFunctions);

                _logger.Trace(correlationId, $"Retrieved {filteredItems.Count} items");

                return await Task.FromResult(filteredItems);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets a list of data items retrieved by given unique ids.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="ids">ids of data items to be retrieved</param>
        /// <returns>a data list.</returns>
        public async Task<List<T>> GetListByIdsAsync(string correlationId, K[] ids)
        {
            _lock.EnterReadLock();

            try
            {
               var filteredItems = _items.FindAll(x => ids.Contains(x.Id));

                _logger.Trace(correlationId, $"Retrieved {filteredItems.Count} items");

                return await Task.FromResult(filteredItems);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets a data item by its unique id.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="id">an id of data item to be retrieved.</param>
        /// <returns>data item.</returns>
        public Task<T> GetOneByIdAsync(string correlationId, K id)
        {
            _lock.EnterReadLock();

            try
            {
                var item = _items.FirstOrDefault(x => x.Id.Equals(id));

                if (item != null)
                    _logger.Trace(correlationId, "Retrieved {0} by {1}", item, id);
                else
                    _logger.Trace(correlationId, "Cannot find {0} by {1}", _typeName, id);

                return Task.FromResult(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets a random item from items that match to a given filter.
        /// 
        /// This method shall be called by a public getOneRandom method from child class
        /// that receives FilterParams and converts them into a filter function.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="matchFunctions">(optional) a filter function to filter items.</param>
        /// <returns>a random item.</returns>
        public async Task<T> GetOneRandomAsync(string correlationId, IList<Func<T, bool>> matchFunctions)
        {
            var filteredItems = Filter(_items, matchFunctions);

            if (filteredItems.Count > 0)
            {
                var randomIndex = new Random().Next(0, filteredItems.Count - 1);
                var result = filteredItems[randomIndex];

                _logger.Trace(correlationId, "Retrieved randomly {0}", result);

                return await Task.FromResult(result);
            }

            _logger.Trace(correlationId, "Cannot randomnly find item by filter");

            return default(T);
        }

        /// <summary>
        /// Creates a data item.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="item">an item to be created.</param>
        /// <returns>created item.</returns>
        public async Task<T> CreateAsync(string correlationId, T item)
        {
            var identifiable = item as IStringIdentifiable;
            if (identifiable != null && item.Id == null)
                ObjectWriter.SetProperty(item, nameof(item.Id), IdGenerator.NextLong());

            _lock.EnterWriteLock();

            try
            {
                _items.Add(item);

                _logger.Trace(correlationId, "Created {0}", item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await SaveAsync(correlationId);

            return item;
        }

        /// <summary>
        /// Sets a data item. If the data item exists it updates it, otherwise it create a new data item.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="item">a item to be set.</param>
        /// <returns>updated item.</returns>
        public async Task<T> SetAsync(string correlationId, T item)
        {
            var identifiable = item as IStringIdentifiable;
            if (identifiable != null && item.Id == null)
                ObjectWriter.SetProperty(item, nameof(item.Id), IdGenerator.NextLong());

            _lock.EnterWriteLock();

            try
            {
                var index = _items.FindIndex(x => x.Id == item.Id);

                if (index < 0) _items.Add(item);
                else _items[index] = item;

                _logger.Trace(correlationId, "Set {0}", item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await SaveAsync(correlationId);

            return item;
        }

        /// <summary>
        /// Updates a data item.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="item">an item to be updated.</param>
        /// <returns>updated item.</returns>
        public async Task<T> UpdateAsync(string correlationId, T item)
        {
            _lock.EnterWriteLock();

            try
            {
                var index = _items.FindIndex(x => x.Id.Equals(item.Id));

                if (index < 0)
                    return default(T);

                _items[index] = item;

                _logger.Trace(correlationId, "Updated {0}", item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await SaveAsync(correlationId);

            return item;
        }

        /// <summary>
        /// Deleted a data item by it's unique id.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="id">an id of the item to be deleted</param>
        /// <returns>deleted item.</returns>
        public async Task<T> DeleteByIdAsync(string correlationId, K id)
        {
            _lock.EnterWriteLock();

            T item = default(T);

            try
            {
                item = _items.Find(x => x.Id.Equals(id));

                if (item == null)
                    return default(T);

                _items.Remove(item);

                _logger.Trace(correlationId, "Deleted {0}", item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await SaveAsync(correlationId);

            return await Task.FromResult(item);
        }

        /// <summary>
        /// Deletes data items that match to a given filter.
        /// 
        /// This method shall be called by a public deleteByFilter method from child
        /// class that receives FilterParams and converts them into a filter function.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="matchFunctions">(optional) a filter function to filter items.</param>
        public async Task DeleteByFilterAsync(string correlationId, IList<Func<T, bool>> matchFunctions)
        {
            var deleted = false;

            _lock.EnterWriteLock();

            try
            {
                var filteredItems = Filter(_items, matchFunctions);
                deleted = filteredItems.Count > 0;

                foreach (var item in filteredItems)
                {
                    _items.Remove(item);
                }

                _logger.Trace(correlationId, $"Deleted {filteredItems.Count} items");
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            if (deleted)
                await SaveAsync(correlationId);
        }

        /// <summary>
        /// Deletes multiple data items by their unique ids.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="ids">ids of data items to be deleted.</param>
        public async Task DeleteByIdsAsync(string correlationId, K[] ids)
        {
            var deleted = false;

            _lock.EnterWriteLock();

            try
            {
                var filteredItems = _items.FindAll(x => ids.Contains(x.Id));
                deleted = filteredItems.Count > 0;

                foreach (var item in filteredItems)
                {
                    _items.Remove(item);
                }

                _logger.Trace(correlationId, $"Deleted {filteredItems.Count} items");
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            if (deleted)
                await SaveAsync(correlationId);
        }

    }
}