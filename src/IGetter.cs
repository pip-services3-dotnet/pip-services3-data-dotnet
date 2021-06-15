using System.Threading;
using System.Threading.Tasks;
using PipServices3.Commons.Data;

namespace PipServices3.Data
{
    /// <summary>
    /// Interface for data processing components that can get data items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public interface IGetter<T, in K>
        where T : IIdentifiable<K>
    {
        /// <summary>
        /// Gets a data items by its unique id.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="id">an id of item to be retrieved.</param>
        /// <returns>an item by its id.</returns>
        Task<T> GetOneByIdAsync(string correlationId, K id);
    }
}
