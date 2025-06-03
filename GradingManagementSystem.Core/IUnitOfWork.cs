using GradingManagementSystem.Core.Repositories.Contact;

namespace GradingManagementSystem.Core
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class; // Gets an instance of a generic repository for the specified entity type.
        Task<int> CompleteAsync(); // Saves all changes made in the context to the database asynchronously.
        //A task representing the asynchronous operation.The task result contains the number of
        // state entries written to the database.
    }
}
