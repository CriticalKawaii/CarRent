using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditBookingStatusPage : Page
    {
        private BookingStatus _bookingStatus = new BookingStatus();

        public EditBookingStatusPage(BookingStatus selectedStatus)
        {
            InitializeComponent();
            if (selectedStatus != null)
            {
                _bookingStatus = selectedStatus;
            }
            DataContext = _bookingStatus;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_bookingStatus.BookingStatus1))
                errors.AppendLine("Введите статус бронирования!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_bookingStatus.BookingStatusID == 0)
            {
                DBEntities.GetContext().BookingStatuses.Add(_bookingStatus);
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