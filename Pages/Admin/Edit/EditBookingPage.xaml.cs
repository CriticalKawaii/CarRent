using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp.Pages.Admin.Edit
{
    /// <summary>
    /// Interaction logic for EditBookingPage.xaml
    /// </summary>
    public partial class EditBookingPage : Page
    {
        private Booking _booking = new Booking();
        public EditBookingPage(Booking selectedBooking)
        {
            InitializeComponent();
            if (selectedBooking != null)
            {
                _booking = selectedBooking;
            }
            DataContext = _booking;
            Loaded += Page_Loaded;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var context = DBEntities.GetContext();
            ComboBoxUsers.ItemsSource = context.Users.ToList();
            ComboBoxVehicles.ItemsSource = context.Vehicles.Where(v => v.Available == true).ToList();
            ComboBoxBookingStatus.ItemsSource = context.BookingStatuses.ToList();
            ComboBoxInsurance.ItemsSource = context.Insurances.ToList();
        }
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (_booking.User == null)
                errors.AppendLine("Выберите пользователя!");
            if (_booking.Vehicle == null)
                errors.AppendLine("Выберите транспорт!");
            if (_booking.BookingStatus == null)
                errors.AppendLine("Выберите статус бронирования!");
            if (_booking.StartDate == null || _booking.StartDate == default)
                errors.AppendLine("Выберите дату начала!");
            if (_booking.EndDate == null || _booking.EndDate == default)
                errors.AppendLine("Выберите дату окончания!");
            if (_booking.StartDate > _booking.EndDate)
                errors.AppendLine("Дата окончания должна быть позже даты начала!");
            if (_booking.TotalCost <= 0)
                errors.AppendLine("Введите корректную стоимость!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_booking.BookingID == 0)
            {
                _booking.CreatedAt = DateTime.Now;
                DBEntities.GetContext().Bookings.Add(_booking);

                if (_booking.BookingStatus.BookingStatus1.ToLower() == "подтверждено")
                {
                    var vehicle = _booking.Vehicle;
                    vehicle.Available = false;
                }
            }
            else
            {
                var vehicle = _booking.Vehicle;
                vehicle.Available = !(_booking.BookingStatus.BookingStatus1.ToLower() == "подтверждено");
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
