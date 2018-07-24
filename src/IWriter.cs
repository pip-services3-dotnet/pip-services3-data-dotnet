using System.Threading.Tasks;

namespace PipServices.Data
{
    public interface IWriter<T, in K>
    {
        Task<T> CreateAsync(string correlationId, T item);
        Task<T> UpdateAsync(string correlationId, T item);
        Task<T> DeleteByIdAsync(string correlationId, K id);
    }
}
