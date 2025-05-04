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
        private List<VehicleImage> _vehicleImages = new List<VehicleImage>();

        public EditVehiclePage(Vehicle selectedVehicle)
        {
            InitializeComponent();

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

        

        
    }
}
