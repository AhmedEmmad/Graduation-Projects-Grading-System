using GradingManagementSystem.Core;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using System.Collections;

namespace GradingManagementSystem.Repository
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly GradingManagementSystemDbContext _dbContext;
        private Hashtable _repositories;

        public UnitOfWork(GradingManagementSystemDbContext dbContext)
        {
            _repositories = new Hashtable();
            _dbContext = dbContext;
        }

        public async Task<int> CompleteAsync() => await _dbContext.SaveChangesAsync();
        
        public async ValueTask DisposeAsync() => await _dbContext.DisposeAsync();
        
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var typeName = typeof(TEntity).Name;

            if(!_repositories.ContainsKey(typeName))
            {
                var repository = new GenericRepository<TEntity>(_dbContext);
                _repositories[typeName] = repository;
            }
            return _repositories[typeName] as IGenericRepository<TEntity>;
        }
    }
}