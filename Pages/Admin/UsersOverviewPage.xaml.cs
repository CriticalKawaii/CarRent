using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfApp.Pages.Admin.Edit;

namespace WpfApp.Pages.admin
{
    public partial class UsersOverviewPage : Page
    {
        public UsersOverviewPage()
        {
            InitializeComponent();
            DataGridUsers.ItemsSource = DBEntities.GetContext().Users.ToList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditUsersPage((sender as Button).DataContext as User));
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditUsersPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
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

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(Visibility == Visibility.Visible)
            {
                DBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(X => X.Reload());
                DataGridUsers.ItemsSource = DBEntities.GetContext().Users.ToList();
            }
        }
    }
}
