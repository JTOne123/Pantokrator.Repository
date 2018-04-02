﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Frost.Data.Sql.Contracts;
using Frost.Data.Sql.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Frost.Data.Sql.Impl
{
    public abstract class ReadRepository<TEntity, TContext> : IEfReadRepository<TEntity>
        where TEntity : BaseEntity
        where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly DbSet<TEntity> _entities;

        #region ctor
        protected ReadRepository(TContext context)
        {
            _context = context;
            _entities = context.Set<TEntity>();
            //if (!(unitOfWork is EfUnitOfWork<TContext> efUnitOfWork)) throw new Exception("Must be EfUnitOfWork"); // TODO: Typed exception
            //_entities = efUnitOfWork.GetDbSet<TEntity>();
        }
        #endregion

        #region Sync Methods              
        public IEnumerable<TEntity> GetAll()
        {
            return _entities.AsNoTracking().AsEnumerable();
        }

        public TEntity GetBy(Func<TEntity, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            var result = _entities.FirstOrDefault(predicate);
            return result;
        }

        public IList<TEntity> GetAllBy(Func<TEntity, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return _entities.Where(predicate).ToList();
        }

        public IEnumerable<TEntity> GetAllBy(
            Expression<Func<TEntity, bool>> filter = null,
            string[] includePaths = null,
            int? page = 0, int? pageSize = null,
            params SortExpression<TEntity>[] sortExpressions)
        {
            IQueryable<TEntity> query = _entities;

            if (filter != null)
            {
                query = query.AsNoTracking().Where(filter);
            }

            if (includePaths != null)
            {
                for (var i = 0; i < includePaths.Count(); i++)
                {
                    query = query.Include(includePaths[i]);
                }
            }

            if (sortExpressions != null)
            {
                IOrderedQueryable<TEntity> orderedQuery = null;
                for (var i = 0; i < sortExpressions.Count(); i++)
                {
                    if (i == 0)
                    {
                        if (sortExpressions[i].SortDirection == ListSortDirection.Ascending)
                        {
                            orderedQuery = query.OrderBy(sortExpressions[i].SortBy);
                        }
                        else
                        {
                            orderedQuery = query.OrderByDescending(sortExpressions[i].SortBy);
                        }
                    }
                    else
                    {
                        if (sortExpressions[i].SortDirection == ListSortDirection.Ascending)
                        {
                            orderedQuery = orderedQuery.ThenBy(sortExpressions[i].SortBy);
                        }
                        else
                        {
                            orderedQuery = orderedQuery.ThenByDescending(sortExpressions[i].SortBy);
                        }

                    }
                }

                if (page != null)
                {
                    query = orderedQuery.Skip(((int)page - 1) * (int)pageSize);
                }
            }

            if (pageSize != null)
            {
                query = query.Take((int)pageSize);
            }

            return query.ToList();
        }


        #endregion

        #region Async Methods          
        public async Task<TEntity> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            return await _entities.FirstOrDefaultAsync(predicate);

        }

        public async Task<IReadOnlyList<TEntity>> GetAllByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return await _entities.Where(predicate).ToListAsync();
        }

        public async Task<IReadOnlyList<TEntity>> GetAllByAsync(Expression<Func<TEntity, bool>> filter, string[] includePaths, int? page = 0, int? pageSize = null, params SortExpression<TEntity>[] sortExpressions)
        {

            IQueryable<TEntity> query = _entities;

            if (filter != null)
            {
                query = query.AsNoTracking().Where(filter);
            }

            if (includePaths != null)
            {
                for (var i = 0; i < includePaths.Count(); i++)
                {
                    query = query.Include(includePaths[i]);
                }
            }

            if (sortExpressions != null)
            {
                IOrderedQueryable<TEntity> orderedQuery = null;
                for (var i = 0; i < sortExpressions.Count(); i++)
                {
                    if (i == 0)
                    {
                        orderedQuery = sortExpressions[i].SortDirection == ListSortDirection.Ascending ? query.OrderBy(sortExpressions[i].SortBy) : query.OrderByDescending(sortExpressions[i].SortBy);
                    }
                    else
                    {
                        orderedQuery = sortExpressions[i].SortDirection == ListSortDirection.Ascending ? orderedQuery.ThenBy(sortExpressions[i].SortBy) : orderedQuery.ThenByDescending(sortExpressions[i].SortBy);
                    }
                }

                if (page != null)
                {
                    if (pageSize != null) query = orderedQuery.Skip(((int)page - 1) * (int)pageSize);
                }
            }

            if (pageSize != null)
            {
                query = query.Take((int)pageSize);
            }
            return await query.ToListAsync();
        }


        #endregion
    }

}