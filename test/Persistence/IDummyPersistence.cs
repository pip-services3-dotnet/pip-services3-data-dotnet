
using System.Collections.Generic;
using System.Threading.Tasks;

using PipServices3.Commons.Data;

namespace PipServices3.Data.Persistence
{
    public interface IDummyPersistence
    {
        Task<Dummy> ModifyAsync(string correlationId, string id, AnyValueMap updateMap);
        Task<Dummy> DeleteAsync(string correlationId, string id);
        Task<Dummy> GetByIdAsync(string correlationId, string id);
        Task<object> GetByIdAsync(string correlationId, string id, ProjectionParams projection);
        Task<DataPage<Dummy>> GetAsync(string correlationId, FilterParams filter, PagingParams paging);
        Task<DataPage<object>> GetAsync(string correlationId, FilterParams filter, PagingParams paging, ProjectionParams projection);
        Task ClearAsync();

        Task<Dummy> CreateAsync(string correlationId, Dummy item);
        Task<Dummy> DeleteByIdAsync(string correlationId, string id);
        Task DeleteByIdsAsync(string correlationId, string[] ids);
        Task<List<Dummy>> GetListByIdsAsync(string correlationId, string[] ids);
        Task<Dummy> GetOneByIdAsync(string correlationId, string id);
        Task<object> GetOneByIdAsync(string correlationId, string id, ProjectionParams projection);
        Task<Dummy> SetAsync(string correlationId, Dummy item);
        Task<Dummy> UpdateAsync(string correlationId, Dummy item);
        Task<Dummy> UpdatePartiallyAsync(string correlationId, string id, AnyValueMap data);
    }
}
