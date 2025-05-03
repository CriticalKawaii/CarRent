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
            var bookingsForRemoving = DataGridBookings.SelectedItems.Cast<Booking>().ToList();
            if (bookingsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({bookingsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().Bookings.RemoveRange(bookingsForRemoving);
                    DBEntities.GetContext().SaveChanges();

                    DataGridBookings.ItemsSource = DBEntities.GetContext().Bookings.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        //Оплаты
        private void ButtonAddPayment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditPaymentPage(null));
        }

        private void ButtonEditPayment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditPaymentPage((sender as Button).DataContext as Payment));
        }

        private void ButtonDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            var paymentsForRemoving = DataGridPayments.SelectedItems.Cast<Payment>().ToList();
            if (paymentsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({paymentsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().Payments.RemoveRange(paymentsForRemoving);
                    DBEntities.GetContext().SaveChanges();
                    DataGridPayments.ItemsSource = DBEntities.GetContext().Payments.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }
        //Отзывы
        private void ButtonAddReview_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditReviewPage(null));
        }

        private void ButtonEditReview_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditReviewPage((sender as Button).DataContext as Review));
        }

        private void ButtonDeleteReview_Click(object sender, RoutedEventArgs e)
        {
            var reviewsForRemoving = DataGridReviews.SelectedItems.Cast<Review>().ToList();
            if (reviewsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({reviewsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().Reviews.RemoveRange(reviewsForRemoving);
                    DBEntities.GetContext().SaveChanges();
                    DataGridReviews.ItemsSource = DBEntities.GetContext().Reviews.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        //Страховки
        private void ButtonAddInsurance_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditInsurancePage(null));
        }

        private void ButtonEditInsurance_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditInsurancePage((sender as Button).DataContext as Insurance));
        }

        private void ButtonDeleteInsurance_Click(object sender, RoutedEventArgs e)
        {
            var insurancesForRemoving = DataGridInsurances.SelectedItems.Cast<Insurance>().ToList();
            if (insurancesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({insurancesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().Insurances.RemoveRange(insurancesForRemoving);
                    DBEntities.GetContext().SaveChanges();
                    DataGridInsurances.ItemsSource = DBEntities.GetContext().Insurances.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        //Категории Транспорта
        private void ButtonAddCategory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditCategoryPage(null));
        }

        private void ButtonEditCategory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditCategoryPage((sender as Button).DataContext as TransportCategory));
        }

        private void ButtonDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            var categoriesForRemoving = DataGridCategories.SelectedItems.Cast<TransportCategory>().ToList();
            if (categoriesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({categoriesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().TransportCategories.RemoveRange(categoriesForRemoving);
                    DBEntities.GetContext().SaveChanges();
                    DataGridCategories.ItemsSource = DBEntities.GetContext().TransportCategories.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        //Статусы бронирований
        private void ButtonAddBookingStatus_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditBookingStatusPage(null));
        }

        private void ButtonEditBookingStatus_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditBookingStatusPage((sender as Button).DataContext as BookingStatus));
        }

        private void ButtonDeleteBookingStatus_Click(object sender, RoutedEventArgs e)
        {
            var statusesForRemoving = DataGridBookingStatuses.SelectedItems.Cast<BookingStatus>().ToList();
            if (statusesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({statusesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().BookingStatuses.RemoveRange(statusesForRemoving);
                    DBEntities.GetContext().SaveChanges();
                    DataGridBookingStatuses.ItemsSource = DBEntities.GetContext().BookingStatuses.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        //Статусы оплат
        private void ButtonAddPayment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditPaymentPage(null));
        }

        private void ButtonEditPayment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditPaymentPage((sender as Button).DataContext as Payment));
        }

        private void ButtonDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            var paymentsForRemoving = DataGridPayments.SelectedItems.Cast<Payment>().ToList();
            if (paymentsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({paymentsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().Payments.RemoveRange(paymentsForRemoving);
                    DBEntities.GetContext().SaveChanges();
                    DataGridPayments.ItemsSource = DBEntities.GetContext().Payments.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }
        //Способы оплат
        private void ButtonAddPaymentMethod_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditPaymentMethodPage(null));
        }

        private void ButtonEditPaymentMethod_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditPaymentMethodPage((sender as Button).DataContext as PaymentMethod));
        }

        private void ButtonDeletePaymentMethod_Click(object sender, RoutedEventArgs e)
        {
            var methodsForRemoving = DataGridPaymentMethods.SelectedItems.Cast<PaymentMethod>().ToList();
            if (methodsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({methodsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().PaymentMethods.RemoveRange(methodsForRemoving);
                    DBEntities.GetContext().SaveChanges();
                    DataGridPaymentMethods.ItemsSource = DBEntities.GetContext().PaymentMethods.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        //Роли
        private void ButtonAddRole_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditRolePage(null));
        }

        private void ButtonEditRole_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditRolePage((sender as Button).DataContext as Role));
        }

        private void ButtonDeleteRole_Click(object sender, RoutedEventArgs e)
        {
            var rolesForRemoving = DataGridRoles.SelectedItems.Cast<Role>().ToList();
            if (rolesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({rolesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DBEntities.GetContext().Roles.RemoveRange(rolesForRemoving);
                    DBEntities.GetContext().SaveChanges();
                    DataGridRoles.ItemsSource = DBEntities.GetContext().Roles.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }


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
