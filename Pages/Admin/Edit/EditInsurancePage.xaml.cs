using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditInsurancePage : Page
    {
        private Insurance _insurance = new Insurance();

        public EditInsurancePage(Insurance selectedInsurance)
        {
            InitializeComponent();
            if (selectedInsurance != null)
            {
                _insurance = selectedInsurance;
            }
            DataContext = _insurance;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_insurance.InsuranceName))
                errors.AppendLine("Введите название!");
            if (string.IsNullOrWhiteSpace(_insurance.InsuranceDetails))
                errors.AppendLine("Введите описание!");
            if (_insurance.InsurancePrice <= 0)
                errors.AppendLine("Введите корректную цену!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_insurance.InsuranceID == 0)
            {
                DBEntities.GetContext().Insurances.Add(_insurance);
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