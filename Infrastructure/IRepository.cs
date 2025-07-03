using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WEBSITE_TRAVELBOOKING.Infrastructure
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter = null,
                  Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                  string includeProperties = "", int? take = null, int? skip = null);
        IEnumerable<TEntity> GetAllWithIncludes(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        params Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[] includeProperties);
        TEntity GetById(int Id);
        TEntity GetByIdGuild(Guid Id);
        void Insert(TEntity entityInsert);
        void Update(TEntity entityUpdate);
        void Delete(TEntity entityDelete);
        void Remove(TEntity entityRemove);
    }
}
