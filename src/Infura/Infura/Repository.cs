namespace Infura
{
    public interface Repository
    {
        T GetById<T>(object id) where T : Aggregate;
        void Save(Aggregate instance);
    }
}