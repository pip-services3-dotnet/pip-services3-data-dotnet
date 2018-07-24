using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipServices.Commons.Config;
using PipServices.Commons.Data;
using PipServices.Commons.Reflect;

namespace PipServices.Data.Persistence
{
    public class IdentifiableMemoryPersistence<T, K> : MemoryPersistence<T>, IReconfigurable,
        IWriter<T, K>, IGetter<T, K>, ISetter<T>
        where T : IIdentifiable<K>
        where K : class
    {
        protected int _maxPageSize = 100;

        public IdentifiableMemoryPersistence()
            : this(null, null)
        { }

        protected IdentifiableMemoryPersistence(ILoader<T> loader, ISaver<T> saver)
            : base(loader, saver)
        { }

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