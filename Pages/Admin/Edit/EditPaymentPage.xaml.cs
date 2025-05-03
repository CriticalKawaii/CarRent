using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditPaymentPage : Page
    {
        private Payment _payment = new Payment();

        public EditPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();
            if (selectedPayment != null)
            {
                _payment = selectedPayment;
            }
            DataContext = _payment;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var context = DBEntities.GetContext();
            ComboBoxBookings.ItemsSource = context.Bookings.ToList();
            ComboBoxPaymentMethods.ItemsSource = context.PaymentMethods.ToList();
            ComboBoxPaymentStatuses.ItemsSource = context.PaymentStatuses.ToList();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (_payment.Booking == null)
                errors.AppendLine("Выберите бронирование!");
            if (_payment.Amount <= 0)
                errors.AppendLine("Введите корректную сумму!");
            if (_payment.PaymentMethod == null)
                errors.AppendLine("Выберите способ оплаты!");
            if (_payment.PaymentStatus == null)
                errors.AppendLine("Выберите статус оплаты!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_payment.PaymentID == 0)
            {
                _payment.CreatedAt = DateTime.Now;
                DBEntities.GetContext().Payments.Add(_payment);
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