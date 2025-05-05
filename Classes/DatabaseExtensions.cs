using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WpfApp.Classes
{

    public static class DatabaseExtensions
    {

        public static async Task<List<T>> GetEntitiesAsync<T>(
            this DbSet<T> dbSet,
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            IQueryable<T> query = dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public static async Task<T> GetEntityAsync<T>(
            this DbSet<T> dbSet,
            Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            IQueryable<T> query = dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.FirstOrDefaultAsync(filter);
        }
        public static async Task<int> SaveChangesWithRetryAsync(this DBEntities context, int maxRetries = 3)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return await context.SaveChangesAsync();
                }
                catch (Exception ex) when (IsTransientError(ex) && retryCount < maxRetries)
                {
                    retryCount++;
                    await Task.Delay(100 * retryCount);
                }
            }
        }

        private static bool IsTransientError(Exception ex)
        {
            string message = ex.Message.ToLower();
            return message.Contains("timeout") || message.Contains("deadlock") || message.Contains("connection");
        }
    }
}