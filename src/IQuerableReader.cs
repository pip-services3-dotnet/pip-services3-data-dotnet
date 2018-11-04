using PipServices3.Commons.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipServices3.Data
{
    /// <summary>
    /// Interface for data processing components that can query a list of data items.
    /// </summary>
    /// <typeparam name="T">the class type</typeparam>
    public interface IQuerableReader<T>
    {
        /// <summary>
        /// Gets a list of data items using a query string.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="query">(optional) a query string</param>
        /// <param name="sort">(optional) sort parameters</param>
        /// <returns>a list of items by query.</returns>
        Task<List<T>> GetListByQueryAsync(string correlationId, string query, SortParams sort);
    }
}
