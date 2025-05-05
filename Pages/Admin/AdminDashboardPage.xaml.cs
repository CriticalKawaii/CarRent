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

    }
}