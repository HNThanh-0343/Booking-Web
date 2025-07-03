using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private WebsiteCmsBookingContext _context;
        private bool disposed = false;
        private Dictionary<Type, object> repositories;

        public UnitOfWork(WebsiteCmsBookingContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // clear repositories
                    if (repositories != null)
                    {
                        repositories.Clear();
                    }

                    // dispose the db context.
                    _context.Dispose();
                }
            }

            disposed = true;
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (repositories == null)
            {
                repositories = new Dictionary<Type, object>();
            }

            var type = typeof(TEntity);

            if (!repositories.ContainsKey(type))
            {
                repositories[type] = new Repository<TEntity>(_context);
            }

            return (IRepository<TEntity>)repositories[type];
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
