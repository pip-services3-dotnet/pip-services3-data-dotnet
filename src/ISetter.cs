using System.Threading.Tasks;

namespace PipServices3.Data
{
    /// <summary>
    /// Interface for data processing components that can set (create or update) data items.
    /// </summary>
    /// <typeparam name="T">the class type</typeparam>
    public interface ISetter<T>
    {
        /// <summary>
        /// Sets a data item. If the data item exists it updates it, otherwise it create a new data item.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="item">a item to be set.</param>
        /// <returns>updated item.</returns>
        Task<T> SetAsync(string correlationId, T item);
    }
}
