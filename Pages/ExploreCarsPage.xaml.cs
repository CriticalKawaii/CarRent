using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp.Classes;
using WpfApp.Pages;

namespace WpfApp
{
    public partial class ExploreCarsPage : Page
    {
        private ObservableCollection<Vehicle> Vehicles { get; set; }
        private MainWindow MainWindow { get; set; }
        private decimal rentCost;

        public ExploreCarsPage()
        {
            InitializeComponent();
            Loaded += ExploreCarsPage_Loaded;
            
        }

        private void ExploreCarsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Vehicles = new ObservableCollection<Vehicle>(DBEntities.GetContext().Vehicles.ToList().Where(x => x.Available == true));
            ListViewExploreCars.ItemsSource = Vehicles;
            ComboBoxSort.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();
            ListViewExploreCars.SelectedIndex = 0;
            ComboBoxInsurance.ItemsSource = DBEntities.GetContext().Insurances.ToList();
            MainWindow = Application.Current.MainWindow as MainWindow;

            DatePickerStart.DisplayDateStart = DateTime.Today.AddDays(1);
            DatePickerStart.SelectedDate = DatePickerStart.DisplayDateStart;
            DatePickerStart.DisplayDateEnd = DateTime.Today.AddMonths(1);
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is ListViewItem viewItem)
            {
                MessageBox.Show(viewItem.Name);
            }
        }

        private void ListViewExploreCars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = ListViewExploreCars.SelectedItem as Vehicle;
            CalculateRentCost();
        }

        private void UpdateItems()
        {
            var allVehicles = Vehicles;

            string searchText = TextBoxSearch.Text?.Trim().ToLowerInvariant() ?? "";

            var filteredItems = allVehicles.Where(x =>
                (x.Make?.ToLowerInvariant().Contains(searchText) ?? false) ||
                (x.Model?.ToLowerInvariant().Contains(searchText) ?? false) ||
                x.Year.ToString().Contains(searchText)).ToList();

            if (ComboBoxSort.SelectedItem is VehicleCategory selectedCategory)
            {
                filteredItems = filteredItems
                    .Where(x => x.VehicleCategoryID == selectedCategory.VehicleCategoryID)
                    .ToList();
            }

            //filteredItems = filteredItems.OrderBy(v => v.DailyRate).ToList(); // or Year, Make, etc.

            ListViewExploreCars.ItemsSource = filteredItems;
            ListViewExploreCars.SelectedIndex = filteredItems.Any() ? 0 : -1;
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateItems();
        }

        private void ComboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateItems();
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxSort.SelectedIndex = -1;
        }

        private void ButtonRent_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsLoggedIn)
            {
                MessageBox.Show("Пожалуйста, войдите в систему для оформления аренды.", "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                if (MainWindow != null)
                {
                    MainWindow.RadioButtonAccount.IsChecked = true;
                }
                return;
            }
            else
            {
                //ListViewExploreCars.IsEnabled = false;
                //TextBoxSearch.IsEnabled = false;
                //ComboBoxSort.IsEnabled = false;
                //ButtonRemove.IsEnabled = false;


            }
        }

        private void DatePickerStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerStart.SelectedDate.HasValue)
            {
                if (DatePickerStart.SelectedDate <= DateTime.Today)
                {
                    DatePickerStart.SelectedDate = DateTime.Today.AddDays(1);
                }

                if (DatePickerStart.SelectedDate >= DatePickerEnd.SelectedDate)
                {
                    DatePickerEnd.SelectedDate = DatePickerStart.SelectedDate.Value.AddDays(1);
                }

                DatePickerEnd.IsEnabled = true;
                DatePickerEnd.DisplayDateStart = DatePickerStart.SelectedDate.Value.AddDays(1);
            }
            else
            {
                DatePickerEnd.SelectedDate = null;
                DatePickerEnd.IsEnabled = false;
            }
            CalculateRentCost();
        }

        private void DatePickerEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerEnd.SelectedDate.HasValue)
            {
                if (DatePickerEnd.SelectedDate <= DatePickerStart.SelectedDate)
                {
                    DatePickerEnd.SelectedDate = DatePickerStart.SelectedDate.Value.AddDays(1);
                }
                ComboBoxInsurance.IsEnabled = true;
                ButtonRent.IsEnabled = true;
            }
            else
            {
                ComboBoxInsurance.SelectedIndex = -1;
                ComboBoxInsurance.IsEnabled = false;
                ButtonRent.IsEnabled = false;
            }
            CalculateRentCost();

        }
        private void ComboBoxInsurance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateRentCost();
        }
        private void CalculateRentCost()
        {
            if (ListViewExploreCars.SelectedItem is Vehicle vehicle && DatePickerEnd.SelectedDate.HasValue && DatePickerStart.SelectedDate.HasValue) {
                TimeSpan rentalDuration = DatePickerEnd.SelectedDate.Value - DatePickerStart.SelectedDate.Value;
                int numberOfDays = rentalDuration.Days;
                rentCost = vehicle.DailyRate * numberOfDays;

                if(ComboBoxInsurance.SelectedItem is Insurance insurance)
                {
                    rentCost = vehicle.DailyRate * numberOfDays + (Decimal)insurance.InsurancePrice;
                }
                
                ButtonRent.Content = "Арендовать за " + rentCost.ToString();
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(X => X.Reload());
            ListViewExploreCars.ItemsSource = new ObservableCollection<Vehicle>(DBEntities.GetContext().Vehicles.ToList().Where(x => x.Available == true));
        }
    }
}
