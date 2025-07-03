namespace WEBSITE_TRAVELBOOKING.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        void Save();
    }
}
