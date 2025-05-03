using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditVehicleCategoryPage : Page
    {
        private VehicleCategory _vehicleCategory = new VehicleCategory();

        public EditVehicleCategoryPage(VehicleCategory selectedCategory)
        {
            InitializeComponent();
            if (selectedCategory != null)
            {
                _vehicleCategory = selectedCategory;
            }
            DataContext = _vehicleCategory;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_vehicleCategory.VehicleCategory1))
                errors.AppendLine("Введите название категории!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_vehicleCategory.VehicleCategoryID == 0)
            {
                DBEntities.GetContext().VehicleCategories.Add(_vehicleCategory);
            }

            try
            {
                DBEntities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}