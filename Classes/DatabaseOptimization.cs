using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace WpfApp.Classes
{
    public static class DatabaseOptimization
    {
        private static Dictionary<string, object> _cache = new Dictionary<string, object>();
        private static DateTime _lastCacheRefresh = DateTime.MinValue;
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);

        public static List<VehicleCategory> GetVehicleCategories()
        {
            RefreshCacheIfNeeded();

            if (!_cache.ContainsKey("VehicleCategories"))
            {
                _cache["VehicleCategories"] = DBEntities.GetContext().VehicleCategories.ToList();
            }

            return (List<VehicleCategory>)_cache["VehicleCategories"];
        }

        public static List<Insurance> GetInsurances()
        {
            RefreshCacheIfNeeded();

            if (!_cache.ContainsKey("Insurances"))
            {
                _cache["Insurances"] = DBEntities.GetContext().Insurances.ToList();
            }

            return (List<Insurance>)_cache["Insurances"];
        }

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

        public static async Task<List<Vehicle>> GetActiveVehiclesAsync()
        {
            var context = DBEntities.GetContext();

            return await context.Vehicles
                .AsNoTracking()
                .Where(v => v.Available)
                .ToListAsync();
        }

        public static async Task<Vehicle> GetVehicleWithDetailsAsync(int vehicleId)
        {
            var context = DBEntities.GetContext();

            return await context.Vehicles
                .Include(v => v.VehicleCategory)
                .Include(v => v.VehicleImages)
                .Include(v => v.Reviews.Select(r => r.User))
                .FirstOrDefaultAsync(v => v.VehicleID == vehicleId);
        }

        public static void ClearCache()
        {
            _cache.Clear();
            _lastCacheRefresh = DateTime.Now;
        }

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