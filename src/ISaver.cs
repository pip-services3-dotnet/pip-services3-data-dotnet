using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipServices.Data
{
    /// <summary>
    /// Interface for data processing components that save data items.
    /// </summary>
    /// <typeparam name="T">the class type</typeparam>
    public interface ISaver<in T>
    {
        /// <summary>
        /// Saves given data items.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="items">a list of items to save.</param>
        Task SaveAsync(string correlationId, IEnumerable<T> items);
    }
}
