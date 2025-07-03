using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Services
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {

        private WebsiteCmsBookingContext _context;
        private DbSet<TEntity> _dbset;
        public Repository(WebsiteCmsBookingContext context)
        {
            _context = context;
            _dbset = context.Set<TEntity>();
        }

        public void Delete(TEntity entityDelete)
        {
            _dbset.Remove(entityDelete);
            _context.SaveChanges();
        }

        public virtual IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "", int? take = null, int? skip = null)
        {
            try
            {
                IQueryable<TEntity> query = _dbset;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

                if (orderBy != null)
                {
                    query = orderBy(query);
                }

                if (skip.HasValue)
                {
                    query = query.Skip(skip.Value);
                }

                if (take.HasValue)
                {
                    query = query.Take(take.Value);
                }
                return query.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                var s = "";
                throw;
            }
        }

        public virtual IEnumerable<TEntity> GetAllWithIncludes(
         Expression<Func<TEntity, bool>> filter = null,
         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
         params Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _dbset;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var include in includeProperties)
            {
                query = include(query);
            }

            if (orderBy != null)
            {
                return orderBy(query).AsNoTracking().ToList();
            }
            else
            {
                return query.AsNoTracking().ToList();
            }
        }

        public virtual TEntity GetById(int Id)
        {
            return _dbset.Find(Id);
        }
        public virtual TEntity GetByIdGuild(Guid Id)
        {
            return _dbset.Find(Id);
        }

        public virtual void Insert(TEntity entityInsert)
        {
            try
            {
                _dbset.Add(entityInsert);//_dbset =Student , _context = db
                _context.SaveChanges();// db.Student.ToList()
                _context.Entry(entityInsert).State = EntityState.Detached; // 👈 Ngăn tracking sau khi insert
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public void Remove(TEntity entityRemove)
        {
            try
            {
                _context.Entry(entityRemove).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public virtual void Update(TEntity entityUpdate)
        {
            try
            {
                _context.Entry(entityUpdate).State = EntityState.Modified;
                _context.SaveChanges();

            }
            catch (Exception Ex)
            {
                //_context.Entry(entityUpdate).State = EntityState.Detached;
                //_context.Attach(entityUpdate);
                //_context.Entry(entityUpdate).State = EntityState.Modified;
                //_context.SaveChanges();
            }
        }
    }
}
