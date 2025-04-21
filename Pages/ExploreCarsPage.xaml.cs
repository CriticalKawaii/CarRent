using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp
{
    public partial class ExploreCarsPage : Page
    {
        public ExploreCarsPage()
        {
            InitializeComponent();
            ListViewExploreCars.ItemsSource = DBEntities.GetContext().Vehicles.ToList();
            ListViewExploreCars.SelectedIndex = 0;

            ComboBoxSort.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();
        }

        private void ListViewItem_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            ListViewItem viewItem = (ListViewItem)sender;
            MessageBox.Show(viewItem.Name);
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
                listBoxReviews.ItemsSource = selectedVehicle.Reviews.ToList();
                if (selectedVehicle.VehicleImage != null)
                {
                    imageVehicle.Source = ByteArrayToImageSource(selectedVehicle.VehicleImage);
                }

            }
        }

        private void UpdateItems()
        {
            var currentItems = DBEntities.GetContext().Vehicles.ToList();
            currentItems = currentItems.Where(x => x.Make.ToLower().Contains(TextBoxSearch.Text.ToLower())).ToList();
            if(ComboBoxSort.SelectedIndex == 0) ListViewExploreCars.ItemsSource = currentItems.Where(x => x.VehicleCategory.VehicleCategory1.ToLower().Contains(ComboBoxSort.Text.ToLower())).ToList();
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
            UpdateItems();
        }
    }
}


//VehicleImage.ImageSource = ByteArrayToImageSource(selectedVehicle.VehicleImage);
//TextBlockMake.Text = $"Make: {selectedVehicle.Make}\nLicense Plate: {selectedVehicle.LicensePlate}";
/*
 ICollectionView view = CollectionViewSource.GetDefaultView(vehicles);
view.SortDescriptions.Add(new SortDescription("Make", ListSortDirection.Ascending));


view.Filter = item =>
{
    Vehicle vehicle = item as Vehicle;
    return vehicle != null && vehicle.AvgRating >= 4;
};
 */