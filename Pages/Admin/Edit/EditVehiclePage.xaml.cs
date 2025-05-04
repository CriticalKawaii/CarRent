using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfApp.Classes;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditVehiclePage : Page
    {
        private Vehicle _vehicle = new Vehicle();
        private VehicleImageManager _imageManager;
        private List<VehicleImage> _vehicleImages = new List<VehicleImage>();

        public EditVehiclePage(Vehicle selectedVehicle)
        {
            InitializeComponent();
            _imageManager = new VehicleImageManager();

            if (selectedVehicle != null)
            {
                _vehicle = selectedVehicle;
            }
            DataContext = _vehicle;
            Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxCategories.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();

            if (_vehicle.VehicleID > 0)
            {
                // Load vehicle images from the VehicleImages table
                _vehicleImages = _imageManager.GetVehicleImages(_vehicle.VehicleID);

                // Load images into the gallery
                await LoadImagesIntoGallery();
            }
        }

        private async System.Threading.Tasks.Task LoadImagesIntoGallery()
        {
            var imageUrls = _vehicleImages.Select(vi => vi.ImagePath).ToList();
            VehicleImageGallery.LoadImages(imageUrls);
        }


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
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

            if (_vehicle.VehicleID == 0)
            {
                _vehicle.CreatedAt = DateTime.Now;
                _vehicle.Available = true;
                DBEntities.GetContext().Vehicles.Add(_vehicle);

                // Save first to get the VehicleID
                try
                {
                    DBEntities.GetContext().SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message);
                    return;
                }
            }
            else
            {
                try
                {
                    DBEntities.GetContext().SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message);
                    return;
                }
            }

            MessageBox.Show("Данные успешно сохранены");
            NavigationService.GoBack();
        }


        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private async void ButtonAddImage_Click(object sender, RoutedEventArgs e)
        {
            if (_vehicle.VehicleID == 0)
            {
                MessageBox.Show("Пожалуйста, сначала сохраните основные данные автомобиля.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var newImage = await _imageManager.BrowseAndUploadImageAsync(_vehicle.VehicleID);
            if (newImage != null)
            {
                _vehicleImages.Add(newImage);
                await LoadImagesIntoGallery();
            }
        }

        private async void ButtonDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            string currentImageUrl = VehicleImageGallery.GetCurrentImageUrl();
            if (string.IsNullOrEmpty(currentImageUrl))
            {
                MessageBox.Show("Нет выбранного изображения для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить это изображение?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var imageToDelete = _vehicleImages.FirstOrDefault(img => img.ImagePath == currentImageUrl);
                if (imageToDelete != null)
                {
                    _imageManager.DeleteVehicleImage(imageToDelete.ImageID);
                    _vehicleImages.Remove(imageToDelete);
                    await LoadImagesIntoGallery();
                }
            }
        }
    }
}
