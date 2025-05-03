using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditPaymentMethodPage : Page
    {
        private PaymentMethod _paymentMethod = new PaymentMethod();

        public EditPaymentMethodPage(PaymentMethod selectedMethod)
        {
            InitializeComponent();
            if (selectedMethod != null)
            {
                _paymentMethod = selectedMethod;
            }
            DataContext = _paymentMethod;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_paymentMethod.PaymentMethod1))
                errors.AppendLine("Введите способ оплаты!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_paymentMethod.PaymentMethodID == 0)
            {
                DBEntities.GetContext().PaymentMethods.Add(_paymentMethod);
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