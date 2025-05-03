using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Pages.Admin.Edit;

namespace WpfApp.Pages.admin
{

    public partial class VehiclesOverviewPage : Page
    {
        public VehiclesOverviewPage()
        {
            InitializeComponent();
            DataGridVehicles.ItemsSource = DBEntities.GetContext().Vehicles.ToList();
        }
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditVehiclePage((sender as Button).DataContext as Vehicle));
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditVehiclePage(null));
        }
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
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

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                DBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(X => X.Reload());
                DataGridVehicles.ItemsSource = DBEntities.GetContext().Vehicles.ToList();
            }
        }

    }
}
