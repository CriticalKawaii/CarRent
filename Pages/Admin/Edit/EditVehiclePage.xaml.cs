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
                if (_vehicleImages.Count > 0)
                {
                    LoadingIndicator.Visibility = Visibility.Visible;
                }

                List<string> imageUrls;

                if (_vehicleImages.Count > 0)
                {
                    imageUrls = _vehicleImages.Select(vi => vi.ImagePath).ToList();
                }
                else if (_vehicle.VehicleID != 0)
                {
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
                    imageUrls = new List<string>();
                }

                imageUrls.AddRange(_tempImagePaths);

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
                var currentImages = _vehicleImages.Select(vi => vi.ImagePath).ToList();
                VehicleImageGallery.LoadImages(currentImages.Concat(_tempImagePaths).ToList());
            }
        }


        private void ButtonDeleteCurrentImage_Click(object sender, RoutedEventArgs e)
        {
            string currentImageUrl = VehicleImageGallery.GetCurrentImageUrl();

            if (string.IsNullOrEmpty(currentImageUrl))
                return;

            if (_tempImagePaths.Contains(currentImageUrl))
            {
                _tempImagePaths.Remove(currentImageUrl);
            }
            else
            {
                var imageToDelete = _vehicleImages.FirstOrDefault(vi => vi.ImagePath == currentImageUrl);
                if (imageToDelete != null)
                {
                    _vehicleImages.Remove(imageToDelete);
                    _imagesToDelete.Add(imageToDelete);
                }
            }

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
                LoadingIndicator.Visibility = Visibility.Visible;
                SavePanel.IsEnabled = false;

                if (_vehicle.VehicleID == 0)
                {
                    _vehicle.CreatedAt = DateTime.Now;
                    _vehicle.Available = true;
                    DBEntities.GetContext().Vehicles.Add(_vehicle);
                    await Task.Run(() => DBEntities.GetContext().SaveChanges());
                }

                if (_tempImagePaths.Count > 0)
                {
                    var imgBBService = new ImgBBService(_imgBBApiKey);
                    var uploadedUrls = await imgBBService.UploadImagesAsync(_tempImagePaths);

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

                foreach (var image in _imagesToDelete)
                {
                    DBEntities.GetContext().VehicleImages.Remove(image);
                }

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
                LoadingIndicator.Visibility = Visibility.Collapsed;
                SavePanel.IsEnabled = true;
            }
        }
        private void ButtonGoBack_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}
