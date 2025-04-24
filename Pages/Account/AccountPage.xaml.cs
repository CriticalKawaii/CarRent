using System.Windows;
using System.Windows.Controls;
using WpfApp.Classes;
using WpfApp.Pages.admin;

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
            if(SessionManager.CurrentUser != null && SessionManager.CurrentUser.RoleID != 1)
            {
                TabItemAdministration.Visibility = Visibility.Visible;
                TabItemReports.Visibility = Visibility.Visible;
            }
            DataContext = SessionManager.CurrentUser;
        }

        private void ButtonUsersPage_Click(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new UsersOverviewPage());
        }

        private void ButtonVehiclesPage_Click(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new VehiclesOverviewPage());
        }

        private void ButtonBookingsPage_Click(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new BookingsOverviewPage());
        }

        private void ButtonReviewsPage_Click(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new ReviewsOverviewPage());
        }

        private void ButtonPaymentsPage_Click(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new PaymentsOverviewPage());
        }

        private void ButtonSignOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.SignOut();
            NavigationService.Navigate(new SignInPage());
        }
    }
}
