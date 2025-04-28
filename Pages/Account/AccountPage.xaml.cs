using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Classes;
using WpfApp.Pages.admin;
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System;


namespace WpfApp.Pages
{
    /// <summary>
    /// Interaction logic for AccountPage.xaml
    /// </summary>
    public partial class AccountPage : Page
    {
        public AccountPage()
        {
            InitializeComponent();
            if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.RoleID != 1)
            {
                TabItemAdministration.Visibility = Visibility.Visible;
                TabItemReports.Visibility = Visibility.Visible;

                ChartBookings.ChartAreas.Add(new ChartArea("Main"));
                var currentSeries = new Series("Бронирования") { 
                    IsValueShownAsLabel = true
                };
                ChartBookings.Series.Add(currentSeries);

                ComboBoxVehicleCategory.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();
                ComboBoxDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
            }
            DataContext = SessionManager.CurrentUser;
            Loaded += AccountPage_Loaded;
        }

        private void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUser != null)
            {
                var userBookings = DBEntities.GetContext().Bookings
                    .Where(b => b.UserID == SessionManager.CurrentUser.UserID)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                DataGridUserBookings.ItemsSource = userBookings;
            }
        }

        private void ButtonViewBookingDetails_Click(object sender, RoutedEventArgs e)
        {
            /*
            var booking = (sender as Button).DataContext as Booking;
            if (booking != null)
            {
                NavigationService.Navigate(new BookingDetailsPage(booking.BookingID));
            }5
            */

            var booking = (sender as Button).DataContext as Booking;
            if (booking != null)
            {
                
                var message = $"Бронирование №{booking.BookingID}\n" +
                              $"Автомобиль: {booking.Vehicle.Make} {booking.Vehicle.Model}\n" +
                              $"Период: {booking.StartDate:dd.MM.yyyy} - {booking.EndDate:dd.MM.yyyy}\n" +
                              $"Стоимость: {booking.TotalCost:C}";

                MessageBox.Show(message, "Детали бронирования", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ButtonUsersPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new UsersOverviewPage());
        }

        private void ButtonVehiclesPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new VehiclesOverviewPage());
        }

        private void ButtonBookingsPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new BookingsOverviewPage());
        }

        private void ButtonReviewsPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new ReviewsOverviewPage());
        }

        private void ButtonPaymentsPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new PaymentsOverviewPage());
        }

        private void ButtonSignOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.SignOut();
            NavigationService.Navigate(new SignInPage());
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxDiagram.SelectedItem is SeriesChartType currentType) {
                Series currentSeries = ChartBookings.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                if (currentSeries == null)
                    return;
                
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var context = DBEntities.GetContext();

                var bookingCounts = context.Bookings
                    .GroupBy(b => b.Vehicle.VehicleCategory.VehicleCategory1)
                    .Select(g => new
                    {
                        VehicleCategory = g.Key,
                        BookingCount = g.Count(),
                    }).ToList();


                foreach (var item in bookingCounts)
                {
                    currentSeries.Points.AddXY(item.VehicleCategory, item.BookingCount);
                    

                    currentSeries.Points.Last().ToolTip = $"{item.VehicleCategory}: {item.BookingCount} бронирований";
                }
            }
        }
    }
}
