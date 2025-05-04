using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using WpfApp.Services;
using System.Diagnostics;
using System.Windows;

namespace WpfApp.Classes
{
    public class VehicleImageManager
    {
        private readonly ImgBBService _imgBBService;

        public VehicleImageManager()
        {
            _imgBBService = new ImgBBService();
        }

        /// <summary>
        /// Uploads an image to ImgBB and creates a database entry
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <param name="imageData">The image data as a byte array</param>
        /// <returns>The created VehicleImage object or null if upload failed</returns>
        public async Task<VehicleImage> UploadVehicleImageAsync(int vehicleId, byte[] imageData)
        {
            try
            {
                // Add debug message
                MessageBox.Show($"Starting image upload for vehicle ID: {vehicleId}", "Debug");

                // Upload image to ImgBB
                string imageUrl = await _imgBBService.UploadImageAsync(imageData, $"vehicle_{vehicleId}_{DateTime.Now.Ticks}");

                // Add debug message
                MessageBox.Show($"Image URL from ImgBB: {imageUrl}", "Debug");

                if (string.IsNullOrEmpty(imageUrl))
                {
                    MessageBox.Show("No URL returned from ImgBB", "Error");
                    return null;
                }

                // Create new VehicleImage in database
                using (var context = new DBEntities())
                {
                    var vehicleImage = new VehicleImage
                    {
                        VehicleID = vehicleId,
                        ImagePath = imageUrl
                    };

                    // Add debug message
                    MessageBox.Show("About to add to context", "Debug");

                    context.VehicleImages.Add(vehicleImage);

                    // Add debug message
                    MessageBox.Show("About to save changes", "Debug");

                    context.SaveChanges();

                    // Add debug message
                    MessageBox.Show($"Saved successfully. New image ID: {vehicleImage.ImageID}", "Debug");

                    return vehicleImage;
                }
            }
            catch (Exception ex)
            {
                // Show detailed error message
                MessageBox.Show($"Error in UploadVehicleImageAsync: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Error");
                return null;
            }
        }
        public bool TestDatabaseConnection(int vehicleId)
        {
            try
            {
                using (var context = new DBEntities())
                {
                    // Test database write operation
                    var testImage = new VehicleImage
                    {
                        VehicleID = vehicleId,
                        ImagePath = "https://test-url.com/test.jpg"
                    };

                    context.VehicleImages.Add(testImage);
                    context.SaveChanges();

                    MessageBox.Show($"Test successful! Created image with ID: {testImage.ImageID}", "Success");

                    // Clean up test data
                    context.VehicleImages.Remove(testImage);
                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database test failed: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Error");
                return false;
            }
        }

        /// <summary>
        /// Gets all images for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>A list of VehicleImage objects</returns>
        public List<VehicleImage> GetVehicleImages(int vehicleId)
        {
            try
            {
                using (var context = new DBEntities())
                {
                    return context.VehicleImages.Where(vi => vi.VehicleID == vehicleId).ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetVehicleImages: {ex}");
                return new List<VehicleImage>();
            }
        }

        /// <summary>
        /// Deletes a vehicle image from the database (doesn't delete from ImgBB)
        /// </summary>
        /// <param name="imageId">The ID of the image to delete</param>
        public void DeleteVehicleImage(int imageId)
        {
            try
            {
                using (var context = new DBEntities())
                {
                    var image = context.VehicleImages.Find(imageId);
                    if (image != null)
                    {
                        context.VehicleImages.Remove(image);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteVehicleImage: {ex}");
                System.Windows.MessageBox.Show($"Error deleting image: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Helper method to browse for an image file and upload it
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>The created VehicleImage object or null if canceled or failed</returns>
        public async Task<VehicleImage> BrowseAndUploadImageAsync(int vehicleId)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
                Title = "Select image to upload"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);
                    return await UploadVehicleImageAsync(vehicleId, imageData);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in BrowseAndUploadImageAsync: {ex}");
                    System.Windows.MessageBox.Show($"Error reading image file: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the primary image for a vehicle (first image or null if none)
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>The primary VehicleImage or null if none exists</returns>
        public VehicleImage GetPrimaryVehicleImage(int vehicleId)
        {
            try
            {
                using (var context = new DBEntities())
                {
                    return context.VehicleImages.FirstOrDefault(vi => vi.VehicleID == vehicleId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetPrimaryVehicleImage: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Migrates an image from the VehicleImage column to the VehicleImages table
        /// </summary>
        /// <param name="vehicle">The vehicle containing the image to migrate</param>
        /// <returns>True if migration was successful, false otherwise</returns>
        public async Task<bool> MigrateVehicleImageAsync(Vehicle vehicle)
        {
            if (vehicle == null || vehicle.VehicleImage == null || vehicle.VehicleImage.Length == 0)
            {
                Debug.WriteLine("Vehicle or image is null or empty");
                return false;
            }

            try
            {
                // Upload the existing image to ImgBB or local storage
                Debug.WriteLine($"Attempting to migrate image for vehicle ID: {vehicle.VehicleID}");
                var vehicleImage = await UploadVehicleImageAsync(vehicle.VehicleID, vehicle.VehicleImage);
                return vehicleImage != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MigrateVehicleImageAsync: {ex}");
                System.Windows.MessageBox.Show($"Error migrating image: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
    }
}