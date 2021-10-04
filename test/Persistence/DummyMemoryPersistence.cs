using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using PipServices3.Commons.Data;

namespace PipServices3.Data.Persistence
{

    public class DummyMemoryPersistence : IdentifiableMemoryPersistence<Dummy, string>, IDummyPersistence
    {
        public DummyMemoryPersistence() : base()
        {
            _maxPageSize = 1000;
        }

        private List<Func<Dummy, bool>> ComposeFilter(FilterParams filter)
        {
            filter ??= new FilterParams();

            var id = filter.GetAsNullableString("id");
            var key = filter.GetAsNullableString("key");
            var ids = filter.GetAsNullableString("ids");
            var idsList = ids != null ? new List<string>(ids.Split(',')) : null;

            return new List<Func<Dummy, bool>>()
            {
                (item) =>
                {
                    if (id != null && item.Id != id)
                        return false;
                    if (key != null && item.Key != key)
                        return false;
                    if (idsList != null && idsList.IndexOf(item.Id) < 0)
                        return false;
                    return true;
                }
            };
        }

        public async Task<DataPage<Dummy>> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            return await base.GetPageByFilterAsync(correlationId, this.ComposeFilter(filter), paging);
        }


        public Task<Dummy> ModifyAsync(string correlationId, string id, AnyValueMap updateMap)
        {
            throw new NotImplementedException();
        }

        public Task<Dummy> DeleteAsync(string correlationId, string id)
        {
            throw new NotImplementedException();
        }

        public Task<Dummy> GetByIdAsync(string correlationId, string id)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetByIdAsync(string correlationId, string id, ProjectionParams projection)
        {
            throw new NotImplementedException();
        }

        public Task<DataPage<Dummy>> GetAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            throw new NotImplementedException();
        }

        public Task<DataPage<object>> GetAsync(string correlationId, FilterParams filter, PagingParams paging, ProjectionParams projection)
        {
            throw new NotImplementedException();
        }

        public Task ClearAsync()
        {
            throw new NotImplementedException();
        }

        public Task<object> GetOneByIdAsync(string correlationId, string id, ProjectionParams projection)
        {
            throw new NotImplementedException();
        }
    }
}
