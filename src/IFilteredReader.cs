using System.Collections.Generic;
using PipServices3.Commons.Data;
using System.Threading.Tasks;

namespace PipServices3.Data
{
    /// <summary>
    /// Interface for data processing components that can retrieve a list of data items by filter.
    /// </summary>
    /// <typeparam name="T">the class type</typeparam>
    public interface IFilteredReader<T>
        where T : class 
    {
        /// <summary>
        /// Gets a list of data items using filter parameters.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="filter">(optional) filter parameters</param>
        /// <param name="sort">(optional) sort parameters</param>
        /// <returns>list of filtered items.</returns>
        Task<List<T>> GetListByFilterAsync(string correlationId, FilterParams filter, SortParams sort);
    }
}
