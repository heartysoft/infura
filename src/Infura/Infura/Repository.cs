using System.Threading.Tasks;

namespace Infura
{
    public interface Repository
    {
        Task<T> GetById<T>(object id) where T : Aggregate;
        Task Save(Aggregate instance);
    }
}