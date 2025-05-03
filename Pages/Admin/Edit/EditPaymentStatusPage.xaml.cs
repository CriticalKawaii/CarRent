using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditPaymentStatusPage : Page
    {
        private PaymentStatus _paymentStatus = new PaymentStatus();

        public EditPaymentStatusPage(PaymentStatus selectedStatus)
        {
            InitializeComponent();
            if (selectedStatus != null)
            {
                _paymentStatus = selectedStatus;
            }
            DataContext = _paymentStatus;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_paymentStatus.PaymentStatus1))
                errors.AppendLine("Введите статус оплаты!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_paymentStatus.PaymentStatusID == 0)
            {
                DBEntities.GetContext().PaymentStatuses.Add(_paymentStatus);
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