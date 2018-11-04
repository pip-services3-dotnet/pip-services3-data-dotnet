using PipServices3.Commons.Data;
using System.Threading.Tasks;

namespace PipServices3.Data
{
    /// <summary>
    /// Interface for data processing components that can query a page of data items.
    /// </summary>
    /// <typeparam name="T">the class type</typeparam>
    public interface IQuarablePageReader<T>
        where T : class
    {
        /// <summary>
        /// Gets a page of data items using a query string.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="query">(optional) a query string</param>
        /// <param name="paging">(optional) paging parameters</param>
        /// <param name="sort">(optional) sort parameters</param>
        /// <returns>a list of items by query.</returns>
        Task<DataPage<T>> GetPageByQueryAsync(string correlationId, string query, PagingParams paging, SortParams sort);
    }
}
