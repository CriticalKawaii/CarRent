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
        public ObservableCollection<Vehicle> Vehicles { get; set; }
        public MainWindow mainWindow { get; set; }
        public ExploreCarsPage()
        {
            InitializeComponent();
            Loaded += ExploreCarsPage_Loaded;
        }

        private void ExploreCarsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Vehicles = new ObservableCollection<Vehicle>(DBEntities.GetContext().Vehicles.ToList());
            ListViewExploreCars.ItemsSource = Vehicles;
            ComboBoxSort.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();
            ListViewExploreCars.SelectedIndex = 0;

            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is ListViewItem viewItem)
            {
                MessageBox.Show(viewItem.Name);
            }
        }

        public ImageSource ByteArrayToImageSource(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            using (var stream = new MemoryStream(imageData))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        private void ListViewExploreCars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = (Vehicle)ListViewExploreCars.SelectedItem;
            if (DataContext is Vehicle selectedVehicle)
            {
                textBlockMakeModel.Text = $"{selectedVehicle.Make} {selectedVehicle.Model} {selectedVehicle.Year}";
                textBlockLicensePlate.Text = $"License: {selectedVehicle.LicensePlate}";
                textBlockDailyRate.Text = $"Daily Rate: {selectedVehicle.DailyRate:C}";

                ListViewReviews.ItemsSource = selectedVehicle.Reviews.ToList();

                if (selectedVehicle.VehicleImage != null)
                {
                    imageVehicle.Source = ByteArrayToImageSource(selectedVehicle.VehicleImage) ?? new BitmapImage(new Uri("pack://application:,,,/Images/placeholder.png"));
                }
            }
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
            if (SessionManager.IsLoggedIn)
            {
                NavigationService.Navigate(new RentPage());
            }
            else if(mainWindow != null)
            {
                mainWindow.RadioButtonAccount.IsChecked = true;
            }
        }
    }
}
