using System.Threading.Tasks;
using PipServices3.Commons.Data;

namespace PipServices3.Data
{
    /// <summary>
    /// Interface for data processing components to update data items partially.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public interface IPartialUpdater<T, in K>
    {
        /// <summary>
        /// Updates only few selected fields in a data item.
        /// </summary>
        /// <param name="correlation_id">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="id">an id of data item to be updated.</param>
        /// <param name="data">a map with fields to be updated.</param>
        /// <returns>updated item.</returns>
        Task<T> UpdatePartially(string correlation_id, K id, AnyValueMap data);
    }
}
