using System;
using System.Data.Entity;
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
            var sources = new (DataGrid Grid, object Data)[]
            {
                (DataGridUsers, context.Users.ToList()),
                (DataGridVehicles, context.Vehicles.ToList()),
                (DataGridBookings, context.Bookings.ToList()),
                (DataGridPayments, context.Payments.ToList()),
                (DataGridReviews, context.Reviews.ToList()),
                (DataGridInsurances, context.Insurances.ToList()),
                (DataGridVehicleCategories, context.VehicleCategories.ToList()),
                (DataGridBookingStatuses, context.BookingStatuses.ToList()),
                (DataGridPaymentStatuses, context.PaymentStatuses.ToList()),
                (DataGridPaymentMethods, context.PaymentMethods.ToList()),
                (DataGridRoles, context.Roles.ToList()),
            };
            foreach (var (grid, data) in sources)
                grid.ItemsSource = (System.Collections.IEnumerable)data;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                DBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(X => X.Reload());
                LoadData();
            }
        }

        private void DeleteSelectedItems<T>(DataGrid grid, DbSet<T> dbSet) where T : class
        {
            var selectedItems = grid.SelectedItems.Cast<T>().ToList();
            if (selectedItems.Any() && MessageBox.Show($"Удалить записи? ({selectedItems.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var context = DBEntities.GetContext();
                    dbSet.RemoveRange(selectedItems);
                    context.SaveChanges();
                    grid.ItemsSource = dbSet.ToList();
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void NavigateToEditPage<T>(object dataContext) where T : Page
        {
            var page = (T)Activator.CreateInstance(typeof(T), dataContext);
            NavigationService.Navigate(page);
        }

        //Обработчики удаления
        private void ButtonDeleteUser_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridUsers, DBEntities.GetContext().Users);
        private void ButtonDeleteVehicle_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridVehicles, DBEntities.GetContext().Vehicles);
        private void ButtonDeleteBooking_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridBookings, DBEntities.GetContext().Bookings);
        private void ButtonDeletePayment_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridPayments, DBEntities.GetContext().Payments);
        private void ButtonDeleteReview_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridReviews, DBEntities.GetContext().Reviews);
        private void ButtonDeleteInsurance_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridInsurances, DBEntities.GetContext().Insurances);
        private void ButtonDeleteVehicleCategory_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridVehicleCategories, DBEntities.GetContext().VehicleCategories);
        private void ButtonDeleteBookingStatus_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridBookingStatuses, DBEntities.GetContext().BookingStatuses);
        private void ButtonDeletePaymentStatus_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridPaymentStatuses, DBEntities.GetContext().PaymentStatuses);
        private void ButtonDeletePaymentMethod_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridPaymentMethods, DBEntities.GetContext().PaymentMethods);
        private void ButtonDeleteRole_Click(object sender, RoutedEventArgs e) => DeleteSelectedItems(DataGridRoles, DBEntities.GetContext().Roles);

        //Обработчики редактирования
        private void ButtonEditUser_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditUserPage>((sender as Button).DataContext);
        private void ButtonEditVehicle_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditVehiclePage>((sender as Button).DataContext);
        private void ButtonEditBooking_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditBookingPage>((sender as Button).DataContext);
        private void ButtonEditPayment_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditPaymentPage>((sender as Button).DataContext);
        private void ButtonEditReview_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditReviewPage>((sender as Button).DataContext);
        private void ButtonEditInsurance_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditInsurancePage>((sender as Button).DataContext);
        private void ButtonEditVehicleCategory_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditVehicleCategoryPage>((sender as Button).DataContext);
        private void ButtonEditBookingStatus_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditBookingStatusPage>((sender as Button).DataContext);
        private void ButtonEditPaymentStatus_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditPaymentStatusPage>((sender as Button).DataContext);
        private void ButtonEditPaymentMethod_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditPaymentMethodPage>((sender as Button).DataContext);
        private void ButtonEditRole_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditRolePage>((sender as Button).DataContext);

        //Обработчики добавления
        private void ButtonAddUser_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditUserPage>(null);
        private void ButtonAddVehicle_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditVehiclePage>(null);
        private void ButtonAddBooking_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditBookingPage>(null);
        private void ButtonAddPayment_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditPaymentPage>(null);
        private void ButtonAddReview_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditReviewPage>(null);
        private void ButtonAddInsurance_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditInsurancePage>(null);
        private void ButtonAddVehicleCategory_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditVehicleCategoryPage>(null);
        private void ButtonAddBookingStatus_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditBookingStatusPage>(null);
        private void ButtonAddPaymentStatus_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditPaymentStatusPage>(null);
        private void ButtonAddPaymentMethod_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditPaymentMethodPage>(null);
        private void ButtonAddRole_Click(object sender, RoutedEventArgs e) => NavigateToEditPage<EditRolePage>(null);
        
        //// Пользователи
        //private void ButtonAddUser_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditUserPage(null));
        //}

        //private void ButtonEditUser_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditUserPage((sender as Button).DataContext as User));
        //}

        //private void ButtonDeleteUser_Click(object sender, RoutedEventArgs e)
        //{
        //    var usersForRemoving = DataGridUsers.SelectedItems.Cast<User>().ToList();
        //    if (usersForRemoving.Any() && MessageBox.Show($"Удалить записи? ({usersForRemoving.Count()})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().Users.RemoveRange(usersForRemoving);
        //            DBEntities.GetContext().SaveChanges();

        //            DataGridUsers.ItemsSource = DBEntities.GetContext().Users.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}

        //// Транспорт
        //private void ButtonAddVehicle_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditVehiclePage(null));
        //}

        //private void ButtonEditVehicle_Click(object sender, RoutedEventArgs e)
        //{
        //    var button = sender as Button;
        //    var vehicle = button.DataContext as Vehicle;
        //    NavigationService.Navigate(new EditVehiclePage(vehicle));
        //}

        //private void ButtonDeleteVehicle_Click(object sender, RoutedEventArgs e)
        //{
        //    var vehiclesForRemoving = DataGridVehicles.SelectedItems.Cast<Vehicle>().ToList();
        //    if (vehiclesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({vehiclesForRemoving.Count()})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().Vehicles.RemoveRange(vehiclesForRemoving);
        //            DBEntities.GetContext().SaveChanges();

        //            DataGridVehicles.ItemsSource = DBEntities.GetContext().Vehicles.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}

        //// Бронирования
        //private void ButtonAddBooking_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditBookingPage(null));
        //}

        //private void ButtonEditBooking_Click(object sender, RoutedEventArgs e)
        //{
        //    var button = sender as Button;
        //    var booking = button.DataContext as Booking;
        //    NavigationService.Navigate(new EditBookingPage(booking));
        //}

        //private void ButtonDeleteBooking_Click(object sender, RoutedEventArgs e)
        //{
        //    var bookingsForRemoving = DataGridBookings.SelectedItems.Cast<Booking>().ToList();
        //    if (bookingsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({bookingsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().Bookings.RemoveRange(bookingsForRemoving);
        //            DBEntities.GetContext().SaveChanges();

        //            DataGridBookings.ItemsSource = DBEntities.GetContext().Bookings.ToList();
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //    }
        //}

        ////Оплаты
        //private void ButtonAddPayment_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditPaymentPage(null));
        //}

        //private void ButtonEditPayment_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditPaymentPage((sender as Button).DataContext as Payment));
        //}

        //private void ButtonDeletePayment_Click(object sender, RoutedEventArgs e)
        //{
        //    var paymentsForRemoving = DataGridPayments.SelectedItems.Cast<Payment>().ToList();
        //    if (paymentsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({paymentsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().Payments.RemoveRange(paymentsForRemoving);
        //            DBEntities.GetContext().SaveChanges();
        //            DataGridPayments.ItemsSource = DBEntities.GetContext().Payments.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}
        ////Отзывы
        //private void ButtonAddReview_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditReviewPage(null));
        //}

        //private void ButtonEditReview_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditReviewPage((sender as Button).DataContext as Review));
        //}

        //private void ButtonDeleteReview_Click(object sender, RoutedEventArgs e)
        //{
        //    var reviewsForRemoving = DataGridReviews.SelectedItems.Cast<Review>().ToList();
        //    if (reviewsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({reviewsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().Reviews.RemoveRange(reviewsForRemoving);
        //            DBEntities.GetContext().SaveChanges();
        //            DataGridReviews.ItemsSource = DBEntities.GetContext().Reviews.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}

        ////Страховки
        //private void ButtonAddInsurance_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditInsurancePage(null));
        //}

        //private void ButtonEditInsurance_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditInsurancePage((sender as Button).DataContext as Insurance));
        //}

        //private void ButtonDeleteInsurance_Click(object sender, RoutedEventArgs e)
        //{
        //    var insurancesForRemoving = DataGridInsurances.SelectedItems.Cast<Insurance>().ToList();
        //    if (insurancesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({insurancesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().Insurances.RemoveRange(insurancesForRemoving);
        //            DBEntities.GetContext().SaveChanges();
        //            DataGridInsurances.ItemsSource = DBEntities.GetContext().Insurances.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}

        ////Категории Транспорта
        //private void ButtonAddVehicleCategory_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditVehicleCategoryPage(null));
        //}

        //private void ButtonEditVehicleCategory_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditVehicleCategoryPage((sender as Button).DataContext as VehicleCategory));
        //}

        //private void ButtonDeleteVehicleCategory_Click(object sender, RoutedEventArgs e)
        //{
        //    var categoriesForRemoving = DataGridVehicleCategories.SelectedItems.Cast<VehicleCategory>().ToList();
        //    if (categoriesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({categoriesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().VehicleCategories.RemoveRange(categoriesForRemoving);
        //            DBEntities.GetContext().SaveChanges();
        //            DataGridVehicleCategories.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}

        ////Статусы бронирований
        //private void ButtonAddBookingStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditBookingStatusPage(null));
        //}

        //private void ButtonEditBookingStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditBookingStatusPage((sender as Button).DataContext as BookingStatus));
        //}

        //private void ButtonDeleteBookingStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    var statusesForRemoving = DataGridBookingStatuses.SelectedItems.Cast<BookingStatus>().ToList();
        //    if (statusesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({statusesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().BookingStatuses.RemoveRange(statusesForRemoving);
        //            DBEntities.GetContext().SaveChanges();
        //            DataGridBookingStatuses.ItemsSource = DBEntities.GetContext().BookingStatuses.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}

        ////Статусы оплат
        //private void ButtonAddPaymentStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditPaymentStatusPage(null));
        //}

        //private void ButtonEditPaymentStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditPaymentStatusPage((sender as Button).DataContext as PaymentStatus));
        //}

        //private void ButtonDeletePaymentStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    var statusesForRemoving = DataGridPaymentStatuses.SelectedItems.Cast<PaymentStatus>().ToList();
        //    if (statusesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({statusesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().PaymentStatuses.RemoveRange(statusesForRemoving);
        //            DBEntities.GetContext().SaveChanges();
        //            DataGridPaymentStatuses.ItemsSource = DBEntities.GetContext().PaymentStatuses.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}

        ////Способы оплат
        //private void ButtonAddPaymentMethod_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditPaymentMethodPage(null));
        //}

        //private void ButtonEditPaymentMethod_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditPaymentMethodPage((sender as Button).DataContext as PaymentMethod));
        //}

        //private void ButtonDeletePaymentMethod_Click(object sender, RoutedEventArgs e)
        //{
        //    var methodsForRemoving = DataGridPaymentMethods.SelectedItems.Cast<PaymentMethod>().ToList();
        //    if (methodsForRemoving.Any() && MessageBox.Show($"Удалить записи? ({methodsForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().PaymentMethods.RemoveRange(methodsForRemoving);
        //            DBEntities.GetContext().SaveChanges();
        //            DataGridPaymentMethods.ItemsSource = DBEntities.GetContext().PaymentMethods.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}

        ////Роли
        //private void ButtonAddRole_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditRolePage(null));
        //}

        //private void ButtonEditRole_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditRolePage((sender as Button).DataContext as Role));
        //}

        //private void ButtonDeleteRole_Click(object sender, RoutedEventArgs e)
        //{
        //    var rolesForRemoving = DataGridRoles.SelectedItems.Cast<Role>().ToList();
        //    if (rolesForRemoving.Any() && MessageBox.Show($"Удалить записи? ({rolesForRemoving.Count})", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        try
        //        {
        //            DBEntities.GetContext().Roles.RemoveRange(rolesForRemoving);
        //            DBEntities.GetContext().SaveChanges();
        //            DataGridRoles.ItemsSource = DBEntities.GetContext().Roles.ToList();
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning); }
        //    }
        //}
    }
}