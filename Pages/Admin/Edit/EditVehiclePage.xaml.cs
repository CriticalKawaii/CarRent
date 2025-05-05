using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfApp.Classes;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditVehiclePage : Page
    {
        private Vehicle _vehicle = new Vehicle();
        private ObservableCollection<VehicleImage> _vehicleImages = new ObservableCollection<VehicleImage>();
        private List<string> _tempImagePaths = new List<string>();
        private List<VehicleImage> _imagesToDelete = new List<VehicleImage>();
        private readonly string _imgBBApiKey = "f4f86515362892ae57c23e650747804b";

        public EditVehiclePage(Vehicle selectedVehicle)
        {
            InitializeComponent();

            if (selectedVehicle != null)
            {
                _vehicle = selectedVehicle;

                if (_vehicle.VehicleID != 0)
                {
                    _vehicleImages = new ObservableCollection<VehicleImage>(
                        DBEntities.GetContext().VehicleImages.Where(vi => vi.VehicleID == _vehicle.VehicleID).ToList());
                }
            }
            DataContext = _vehicle;
            Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxCategories.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();

            await LoadImagesIntoGallery();
        }

        private async Task LoadImagesIntoGallery()
        {
            try
            {
                // Show loading indicator if there are images to load
                if (_vehicleImages.Count > 0)
                {
                    LoadingIndicator.Visibility = Visibility.Visible;
                }

                List<string> imageUrls;

                if (_vehicleImages.Count > 0)
                {
                    // Use existing vehicle images
                    imageUrls = _vehicleImages.Select(vi => vi.ImagePath).ToList();
                }
                else if (_vehicle.VehicleID != 0)
                {
                    // Try loading from database if not already loaded
                    using (var context = new DBEntities())
                    {
                        var images = await context.VehicleImages
                            .Where(vi => vi.VehicleID == _vehicle.VehicleID)
                            .ToListAsync();

                        _vehicleImages.Clear();
                        foreach (var img in images)
                        {
                            _vehicleImages.Add(img);
                        }

                        imageUrls = images.Select(vi => vi.ImagePath).ToList();
                    }
                }
                else
                {
                    // New vehicle, no images yet
                    imageUrls = new List<string>();
                }

                // Add temp images to the display list
                imageUrls.AddRange(_tempImagePaths);

                // Load images in the gallery
                VehicleImageGallery.LoadImages(imageUrls);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async void ButtonAddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp) | *.jpg; *.jpeg; *.png; *.bmp",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (string filename in dialog.FileNames)
                {
                    _tempImagePaths.Add(filename);
                }

                // Show the temp images in the gallery
                var currentImages = _vehicleImages.Select(vi => vi.ImagePath).ToList();
                VehicleImageGallery.LoadImages(currentImages.Concat(_tempImagePaths).ToList());
            }
        }


        private void ButtonDeleteCurrentImage_Click(object sender, RoutedEventArgs e)
        {
            // Get current image
            string currentImageUrl = VehicleImageGallery.GetCurrentImageUrl();

            if (string.IsNullOrEmpty(currentImageUrl))
                return;

            // Check if it's a temp image or a saved one
            if (_tempImagePaths.Contains(currentImageUrl))
            {
                _tempImagePaths.Remove(currentImageUrl);
            }
            else
            {
                // Find the VehicleImage entity
                var imageToDelete = _vehicleImages.FirstOrDefault(vi => vi.ImagePath == currentImageUrl);
                if (imageToDelete != null)
                {
                    _vehicleImages.Remove(imageToDelete);
                    _imagesToDelete.Add(imageToDelete);
                }
            }

            // Refresh gallery
            var remainingImages = _vehicleImages.Select(vi => vi.ImagePath).Concat(_tempImagePaths).ToList();
            VehicleImageGallery.LoadImages(remainingImages);
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (_vehicle.VehicleCategory == null)
                errors.AppendLine("Выберите категорию!");
            if (string.IsNullOrWhiteSpace(_vehicle.Make))
                errors.AppendLine("Введите производителя!");
            if (string.IsNullOrWhiteSpace(_vehicle.Model))
                errors.AppendLine("Введите модель!");
            if (_vehicle.Year <= 0)
                errors.AppendLine("Введите корректный год!");
            if (string.IsNullOrWhiteSpace(_vehicle.LicensePlate))
                errors.AppendLine("Введите номер!");
            if (_vehicle.DailyRate <= 0)
                errors.AppendLine("Введите корректную цену за день!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                // Show loading indicator
                LoadingIndicator.Visibility = Visibility.Visible;
                SavePanel.IsEnabled = false;

                // Save vehicle first to get ID if new
                if (_vehicle.VehicleID == 0)
                {
                    _vehicle.CreatedAt = DateTime.Now;
                    _vehicle.Available = true;
                    DBEntities.GetContext().Vehicles.Add(_vehicle);
                    await Task.Run(() => DBEntities.GetContext().SaveChanges());
                }

                // Upload new images to ImgBB
                if (_tempImagePaths.Count > 0)
                {
                    var imgBBService = new ImgBBService(_imgBBApiKey);
                    var uploadedUrls = await imgBBService.UploadImagesAsync(_tempImagePaths);

                    // Save uploaded URLs to database
                    foreach (var url in uploadedUrls)
                    {
                        var newImage = new VehicleImage
                        {
                            VehicleID = _vehicle.VehicleID,
                            ImagePath = url
                        };
                        DBEntities.GetContext().VehicleImages.Add(newImage);
                    }
                }

                // Delete images marked for deletion
                foreach (var image in _imagesToDelete)
                {
                    DBEntities.GetContext().VehicleImages.Remove(image);
                }

                // Save all changes
                await Task.Run(() => DBEntities.GetContext().SaveChanges());

                MessageBox.Show("Данные успешно сохранены");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
            finally
            {
                // Hide loading indicator
                LoadingIndicator.Visibility = Visibility.Collapsed;
                SavePanel.IsEnabled = true;
            }
        }

        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

    }
}
