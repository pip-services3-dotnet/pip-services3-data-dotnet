using System.Threading.Tasks;
using PipServices.Commons.Data;

namespace PipServices.Data
{
    public interface IPartialUpdater<T, in K>
    {
        Task<T> UpdatePartially(string correlation_id, K id, AnyValueMap data);
    }
}
