using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace WpfApp.Classes
{
    public static class DatabaseOptimization
    {
        // Cache for frequently accessed static data
        private static Dictionary<string, object> _cache = new Dictionary<string, object>();
        private static DateTime _lastCacheRefresh = DateTime.MinValue;
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets all vehicle categories with caching to reduce database calls
        /// </summary>
        public static List<VehicleCategory> GetVehicleCategories()
        {
            RefreshCacheIfNeeded();

            if (!_cache.ContainsKey("VehicleCategories"))
            {
                _cache["VehicleCategories"] = DBEntities.GetContext().VehicleCategories.ToList();
            }

            return (List<VehicleCategory>)_cache["VehicleCategories"];
        }

        /// <summary>
        /// Gets all insurance options with caching
        /// </summary>
        public static List<Insurance> GetInsurances()
        {
            RefreshCacheIfNeeded();

            if (!_cache.ContainsKey("Insurances"))
            {
                _cache["Insurances"] = DBEntities.GetContext().Insurances.ToList();
            }

            return (List<Insurance>)_cache["Insurances"];
        }

        /// <summary>
        /// Gets vehicle images for the specified vehicle
        /// </summary>
        public static List<string> GetVehicleImageUrls(int vehicleId)
        {
            var imageKey = $"VehicleImages_{vehicleId}";
            RefreshCacheIfNeeded();

            if (!_cache.ContainsKey(imageKey))
            {
                _cache[imageKey] = DBEntities.GetContext().VehicleImages
                    .Where(vi => vi.VehicleID == vehicleId)
                    .Select(vi => vi.ImagePath)
                    .ToList();
            }

            return (List<string>)_cache[imageKey];
        }

        /// <summary>
        /// Gets all active vehicles with optimization for performance
        /// </summary>
        public static async Task<List<Vehicle>> GetActiveVehiclesAsync()
        {
            var context = DBEntities.GetContext();

            // Use AsNoTracking for read-only operations to improve performance
            return await context.Vehicles
                .AsNoTracking()
                .Where(v => v.Available)
                .ToListAsync();
        }

        /// <summary>
        /// Efficient loading of vehicle with related data
        /// </summary>
        public static async Task<Vehicle> GetVehicleWithDetailsAsync(int vehicleId)
        {
            var context = DBEntities.GetContext();

            // Use Include to eager load related data in one query
            return await context.Vehicles
                .Include(v => v.VehicleCategory)
                .Include(v => v.VehicleImages)
                .Include(v => v.Reviews.Select(r => r.User))
                .FirstOrDefaultAsync(v => v.VehicleID == vehicleId);
        }

        /// <summary>
        /// Clear the cache to force reload from database
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
            _lastCacheRefresh = DateTime.Now;
        }

        /// <summary>
        /// Check if cache needs refreshing based on lifetime
        /// </summary>
        private static void RefreshCacheIfNeeded()
        {
            if (DateTime.Now - _lastCacheRefresh > CacheLifetime)
            {
                _cache.Clear();
                _lastCacheRefresh = DateTime.Now;
            }
        }
    }
}