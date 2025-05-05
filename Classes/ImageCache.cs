using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfApp.Classes
{
    public static class ImageCache
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> _imageCache = new ConcurrentDictionary<string, BitmapImage>();
        private static readonly ConcurrentDictionary<string, Task<BitmapImage>> _loadingTasks = new ConcurrentDictionary<string, Task<BitmapImage>>();
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private static readonly string _cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RentACar", "ImageCache");

        static ImageCache()
        {
            if (!Directory.Exists(_cacheFolder))
            {
                Directory.CreateDirectory(_cacheFolder);
            }
        }
        public static async Task<BitmapImage> GetImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return GetPlaceholderImage();
            }

            if (_imageCache.TryGetValue(imageUrl, out BitmapImage cachedImage))
            {
                return cachedImage;
            }

            if (_loadingTasks.TryGetValue(imageUrl, out Task<BitmapImage> loadingTask))
            {
                return await loadingTask;
            }

            var newLoadingTask = LoadImageAsync(imageUrl);
            
            _loadingTasks[imageUrl] = newLoadingTask;
            
            try
            {
                var result = await newLoadingTask;
                
                _imageCache[imageUrl] = result;
                
                _loadingTasks.TryRemove(imageUrl, out _);
                
                return result;
            }
            catch (Exception ex)
            {
                _loadingTasks.TryRemove(imageUrl, out _);
                Console.WriteLine($"Error loading image from {imageUrl}: {ex.Message}");
                return GetPlaceholderImage();
            }
        }

        private static async Task<BitmapImage> LoadImageAsync(string imageUrl)
        {
            try
            {
                if (File.Exists(imageUrl))
                {
                    return LoadImageFromFile(imageUrl);
                }
                
                string cacheFileName = GetCacheFileName(imageUrl);
                string cachePath = Path.Combine(_cacheFolder, cacheFileName);

                if (File.Exists(cachePath))
                {
                    return LoadImageFromFile(cachePath);
                }

                byte[] imageData = await _httpClient.GetByteArrayAsync(imageUrl);

                File.WriteAllBytes(cachePath, imageData);

                return CreateBitmapImage(imageData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image from {imageUrl}: {ex.Message}");
                throw;
            }
        }

        public static void ClearCache()
        {
            _imageCache.Clear();
            _loadingTasks.Clear();

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
            try
            {
                var image = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/car_placeholder.png"));
                image.Freeze();
                return image;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading placeholder: {ex.Message}");
                return new BitmapImage();
            }
        }

        private static string GetCacheFileName(string url)
        {
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
            image.Freeze();
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
                image.Freeze(); 
            }
            return image;
        }
    }
}
