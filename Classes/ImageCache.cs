using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfApp.Classes
{
    /// <summary>
    /// Provides caching capabilities for images to improve performance
    /// </summary>
    public static class ImageCache
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> _imageCache = new ConcurrentDictionary<string, BitmapImage>();
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RentACar", "ImageCache");

        static ImageCache()
        {
            // Ensure cache directory exists
            if (!Directory.Exists(_cacheFolder))
            {
                Directory.CreateDirectory(_cacheFolder);
            }
        }

        /// <summary>
        /// Gets an image from cache or downloads it asynchronously
        /// </summary>
        public static async Task<BitmapImage> GetImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return GetPlaceholderImage();
            }

            // Try to get from memory cache first
            if (_imageCache.TryGetValue(imageUrl, out BitmapImage cachedImage))
            {
                return cachedImage;
            }

            try
            {
                // Generate a cache file path (using a hash of the URL)
                string cacheFileName = GetCacheFileName(imageUrl);
                string cachePath = Path.Combine(_cacheFolder, cacheFileName);

                // Check if the image exists in disk cache
                if (File.Exists(cachePath))
                {
                    var image = LoadImageFromFile(cachePath);
                    _imageCache.TryAdd(imageUrl, image);
                    return image;
                }

                // Download the image
                byte[] imageData = await _httpClient.GetByteArrayAsync(imageUrl);

                // Save to disk cache
                File.WriteAllBytes(cachePath, imageData);

                // Create and cache the image
                var bitmapImage = CreateBitmapImage(imageData);
                _imageCache.TryAdd(imageUrl, bitmapImage);
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image from {imageUrl}: {ex.Message}");
                return GetPlaceholderImage();
            }
        }

        /// <summary>
        /// Clears the image cache from memory and disk
        /// </summary>
        public static void ClearCache()
        {
            _imageCache.Clear();

            try
            {
                foreach (var file in Directory.GetFiles(_cacheFolder))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cache: {ex.Message}");
            }
        }

        private static BitmapImage GetPlaceholderImage()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/car_placeholder.png"));
        }

        private static string GetCacheFileName(string url)
        {
            // Create a hash of the URL to use as a filename
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(url);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower() + ".cache";
            }
        }

        private static BitmapImage LoadImageFromFile(string filePath)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(filePath, UriKind.Absolute);
            image.EndInit();
            image.Freeze(); // Make it thread-safe
            return image;
        }

        private static BitmapImage CreateBitmapImage(byte[] imageData)
        {
            var image = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze(); // Make it thread-safe
            }
            return image;
        }
    }
}