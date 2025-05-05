using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WpfApp.Classes
{
    /// <summary>
    /// Extension methods for database operations to improve performance and async behavior
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Gets entities asynchronously with optional filtering, ordering, and includes
        /// </summary>
        public static async Task<List<T>> GetEntitiesAsync<T>(
            this DbSet<T> dbSet,
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            IQueryable<T> query = dbSet;

            // Apply includes for eager loading
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            // Apply filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply ordering
            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        /// <summary>
        /// Gets a single entity asynchronously with optional includes
        /// </summary>
        public static async Task<T> GetEntityAsync<T>(
            this DbSet<T> dbSet,
            Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            IQueryable<T> query = dbSet;

            // Apply includes for eager loading
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.FirstOrDefaultAsync(filter);
        }

        /// <summary>
        /// Saves changes asynchronously with automatic retry for transient failures
        /// </summary>
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
                    // Wait before retrying
                    await Task.Delay(100 * retryCount);
                }
            }
        }

        private static bool IsTransientError(Exception ex)
        {
            // You can add more specific transient error detection here if needed
            string message = ex.Message.ToLower();
            return message.Contains("timeout") || message.Contains("deadlock") || message.Contains("connection");
        }
    }
}