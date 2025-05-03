using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfApp.Pages.Admin.Edit;

namespace WpfApp.Pages.Admin
{
    public partial class AdminDashboardPage : Page
    {
        public AdminDashboardPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var context = DBEntities.GetContext();
            DataGridUsers.ItemsSource = context.Users.ToList();
            DataGridVehicles.ItemsSource = context.Vehicles.ToList();
            DataGridBookings.ItemsSource = context.Bookings.ToList();
        }

        // Пользователи
        private void ButtonAddUser_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditUserPage(null));
        }

        private void ButtonEditUser_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditUserPage((sender as Button).DataContext as User));
        }

        private void ButtonDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var usersForRemoving = DataGridUsers.SelectedItems.Cast<User>().ToList();
            if (usersForRemoving.Any() && MessageBox.Show($"Удалить записи? ({usersForRemoving.Count()})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().Users.RemoveRange(usersForRemoving);
                    DBEntities.GetContext().SaveChanges();

                    DataGridUsers.ItemsSource = DBEntities.GetContext().Users.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        // Транспорт
        private void ButtonAddVehicle_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditVehiclePage(null));
        }

        private void ButtonEditVehicle_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var vehicle = button.DataContext as Vehicle;
            NavigationService.Navigate(new EditVehiclePage(vehicle));
        }

        private void ButtonDeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            var vehiclesForRemoving = DataGridVehicles.SelectedItems.Cast<Vehicle>().ToList();
            if (vehiclesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({vehiclesForRemoving.Count()})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().Vehicles.RemoveRange(vehiclesForRemoving);
                    DBEntities.GetContext().SaveChanges();

                    DataGridVehicles.ItemsSource = DBEntities.GetContext().Vehicles.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        // Бронирования
        private void ButtonAddBooking_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditBookingPage(null));
        }

        private void ButtonEditBooking_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var booking = button.DataContext as Booking;
            NavigationService.Navigate(new EditBookingPage(booking));
        }

        private void ButtonDeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            
        }

        //Оплаты
        //Отзывы
        //Страховки
        //Категории Транспорта
        //Статусы бронирований
        //Статусы оплат
        //Способы оплат
        //Роли

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                DBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(X => X.Reload());
                
                DataGridUsers.ItemsSource = DBEntities.GetContext().Users.ToList();
                DataGridVehicles.ItemsSource = DBEntities.GetContext().Vehicles.ToList();
                DataGridBookings.ItemsSource = DBEntities.GetContext().Bookings.ToList();
            }
        }
    }
}
