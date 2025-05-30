using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Classes;
using System.Collections.Generic;
using WpfApp.Controls;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Data.Entity;

namespace WpfApp
{
    public partial class ExploreCarsPage : Page
    {
        private class SortOption
        {
            public string Name { get; set; }
            public Func<IEnumerable<Vehicle>, IEnumerable<Vehicle>> SortFunction { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
        private ObservableCollection<Vehicle> Vehicles { get; set; }
        private MainWindow MainWindow { get; set; }
        private decimal rentCost;
        private List<SortOption> sortOptions;

        private GalleryDialog _galleryDialog;
        private Grid _dialogHost;

        public ExploreCarsPage()
        {
            InitializeComponent();


            Vehicles = new ObservableCollection<Vehicle>();
            ListViewExploreCars.ItemsSource = Vehicles;

            InitializeSortOptions();
            ComboBoxSort.ItemsSource = sortOptions;

            MainWindow = Application.Current.MainWindow as MainWindow;

            Loaded += ExploreCarsPage_Loaded;
            imageVehicle.MouseLeftButtonDown += ImageVehicle_MouseLeftButtonDown;

            _dialogHost = new Grid();
            _dialogHost.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(150, 0, 0, 0));
            _dialogHost.Visibility = Visibility.Collapsed;
            Grid.SetColumnSpan(_dialogHost, 2);
            Grid.SetRowSpan(_dialogHost, 3);
            this.RegisterName("DialogHost", _dialogHost);

            (this.Content as Grid).Children.Add(_dialogHost);
        }

        private async void ExploreCarsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            
            await LoadVehicleCategoriesAsync();

            DatePickerStart.DisplayDateStart = DateTime.Today.AddDays(1);
            DatePickerStart.DisplayDateEnd = DateTime.Today.AddMonths(1);
            
            await LoadVehiclesAsync();
            await LoadInsurancesAsync();

            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        private async Task LoadVehicleCategoriesAsync()
        {
            try
            {
                using (var context = new DBEntities())
                {
                    var categories = await context.VehicleCategories.ToListAsync();
                    ComboBoxFilter.ItemsSource = categories;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий транспортных средств: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadInsurancesAsync()
        {
            try
            {
                using (var context = new DBEntities())
                {
                    var insurances = await context.Insurances.ToListAsync();
                    ComboBoxInsurance.ItemsSource = insurances;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке страховок: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadBlackoutDatesAsync(int vehicleId)
        {
            try
            {
                DatePickerStart.BlackoutDates.Clear();
                DatePickerEnd.BlackoutDates.Clear();

                DateTime today = DateTime.Today;
                DatePickerStart.BlackoutDates.Add(new CalendarDateRange(DateTime.MinValue, today));
                DatePickerEnd.BlackoutDates.Add(new CalendarDateRange(DateTime.MinValue, today));

                using (var context = new DBEntities())
                {
                    var confirmedBookings = await context.Bookings
                        .Where(x => x.BookingStatus.BookingStatus1 == "Confirmed" && x.VehicleID == vehicleId)
                        .ToListAsync();

                    foreach (var booking in confirmedBookings)
                    {
                        if (booking.StartDate <= booking.EndDate)
                        {
                            DatePickerStart.BlackoutDates.Add(new CalendarDateRange(booking.StartDate, booking.EndDate));
                            DatePickerEnd.BlackoutDates.Add(new CalendarDateRange(booking.StartDate, booking.EndDate));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке дат бронирования: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task LoadVehiclesAsync()
        {
            LoadingProgressBar.Visibility = Visibility.Visible;

            try
            {
                using (var context = new DBEntities())
                {
                    var vehicles = await context.Vehicles
                        .AsNoTracking()
                        .Include(v => v.VehicleCategory)
                        .Include(v => v.VehicleImages)
                        .Where(x => x.Available == true)
                        .ToListAsync();

                    ApplyFiltersAndSort(vehicles);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки транспортных средств: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }


        private async void ImageVehicle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Vehicle selectedVehicle = DataContext as Vehicle;
            if (selectedVehicle != null && selectedVehicle.VehicleID > 0)
            {
                await ShowGalleryDialogAsync(selectedVehicle);
            }
        }

        private async Task ShowGalleryDialogAsync(Vehicle vehicle)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;

            try
            {
                string[] imageUrls = await vehicle.GetAllImageUrlsAsync();

                if (imageUrls == null || imageUrls.Length == 0)
                {
                    MessageBox.Show("Нет изображений для этого автомобиля", "Галерея", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _galleryDialog = new GalleryDialog();
                _galleryDialog.SetTitle($"{vehicle.Make} {vehicle.Model} {vehicle.Year} - Изображения");

                _galleryDialog.LoadImages(imageUrls.ToList());

                _galleryDialog.CloseRequested += GalleryDialog_CloseRequested;

                _dialogHost.Children.Clear();
                _dialogHost.Children.Add(_galleryDialog);
                _dialogHost.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке галереи: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }


        private void GalleryDialog_CloseRequested(object sender, EventArgs e)
        {
            _dialogHost.Visibility = Visibility.Collapsed;
            _dialogHost.Children.Clear();
            _galleryDialog = null;
        }


        private void InitializeSortOptions()
        {
            sortOptions = new List<SortOption>
            {
                new SortOption
                {
                    Name = "Цена: от низкой к высокой",
                    SortFunction = vehicles => vehicles.OrderBy(v => v.DailyRate)
                },
                new SortOption
                {
                    Name = "Цена: от высокой к низкой",
                    SortFunction = vehicles => vehicles.OrderByDescending(v => v.DailyRate)
                },
                new SortOption
                {
                    Name = "Год: сначала новые",
                    SortFunction = vehicles => vehicles.OrderByDescending(v => v.Year)
                },
                new SortOption
                {
                    Name = "Год: сначала старые",
                    SortFunction = vehicles => vehicles.OrderBy(v => v.Year)
                },
                new SortOption
                {
                    Name = "Рейтинг: от высокого к низкому",
                    SortFunction = vehicles => vehicles.OrderByDescending(v => v.AvgRating)
                },
                new SortOption
                {
                    Name = "Рейтинг: от низкого к высокому",
                    SortFunction = vehicles => vehicles.OrderBy(v => v.AvgRating)
                },
                new SortOption
                {
                    Name = "Марка: А-Я",
                    SortFunction = vehicles => vehicles.OrderBy(v => v.Make)
                },
                new SortOption
                {
                    Name = "Марка: Я-А",
                    SortFunction = vehicles => vehicles.OrderByDescending(v => v.Make)
                }
            };
        }

        private async void ListViewExploreCars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vehicle selectedVehicle = ListViewExploreCars.SelectedItem as Vehicle;
            if (selectedVehicle == null) return;

            LoadingProgressBar.Visibility = Visibility.Visible;

            try
            {
                DataContext = selectedVehicle;

                var imageSource = await selectedVehicle.GetImageSourceAsync();
                imageVehicle.Source = imageSource;

                await LoadVehicleReviewsAsync(selectedVehicle);

                DatePickerStart.SelectedDate = null;
                DatePickerEnd.SelectedDate = null;
                await LoadBlackoutDatesAsync(selectedVehicle.VehicleID);
                CalculateRentCost();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных о транспортном средстве: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadVehicleReviewsAsync(Vehicle vehicle)
        {
            try
            {
                using (var context = new DBEntities())
                {
                    var reviews = await context.Reviews
                        .Include(r => r.User)
                        .Where(r => r.VehicleID == vehicle.VehicleID)
                        .OrderByDescending(r => r.CreatedAt)
                        .ToListAsync();

                    ListViewReviews.ItemsSource = reviews;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке отзывов: {ex.Message}");
            }
        }

        private async Task PreloadThumbnailsAsync(IEnumerable<Vehicle> vehicles)
        {
            var loadingTasks = vehicles.Select(v => v.GetImageSourceAsync()).ToList();

            await Task.WhenAll(loadingTasks);
        }

        private async void UpdateItemsAsync()
        {
            LoadingProgressBar.Visibility = Visibility.Visible;

            try
            {
                using (var context = new DBEntities())
                {
                    var query = context.Vehicles
                        .AsNoTracking()
                        .Include(v => v.VehicleCategory)
                        .Include(v => v.VehicleImages)
                        .Where(x => x.Available == true);

                    string searchText = TextBoxSearch.Text?.Trim().ToLowerInvariant() ?? "";
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        query = query.Where(x =>
                            x.Make.ToLower().Contains(searchText) ||
                            x.Model.ToLower().Contains(searchText) ||
                            x.Year.ToString().Contains(searchText));
                    }

                    if (ComboBoxFilter.SelectedItem is VehicleCategory selectedCategory)
                    {
                        query = query.Where(x => x.VehicleCategoryID == selectedCategory.VehicleCategoryID);
                    }

                    var vehicles = await query.ToListAsync();

                    if (ComboBoxSort.SelectedItem is SortOption selectedSort)
                    {
                        vehicles = selectedSort.SortFunction(vehicles).ToList();
                    }
                    await PreloadThumbnailsAsync(vehicles);
                    Dispatcher.Invoke(() =>
                    {
                        Vehicles.Clear();
                        foreach (var vehicle in vehicles)
                        {
                            Vehicles.Add(vehicle);
                        }

                        ListViewExploreCars.SelectedIndex = vehicles.Any() ? 0 : -1;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления транспортных средств: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplyFiltersAndSort(List<Vehicle> vehicles)
        {
            string searchText = TextBoxSearch.Text?.Trim().ToLowerInvariant() ?? "";

            var filteredItems = vehicles;

            if (!string.IsNullOrEmpty(searchText))
            {
                filteredItems = filteredItems
                    .Where(x =>
                        (x.Make?.ToLowerInvariant().Contains(searchText) ?? false) ||
                        (x.Model?.ToLowerInvariant().Contains(searchText) ?? false) ||
                        x.Year.ToString().Contains(searchText))
                    .ToList();
            }

            if (ComboBoxFilter.SelectedItem is VehicleCategory selectedCategory)
            {
                filteredItems = filteredItems
                    .Where(x => x.VehicleCategoryID == selectedCategory.VehicleCategoryID)
                    .ToList();
            }

            if (ComboBoxSort.SelectedItem is SortOption selectedSort)
            {
                filteredItems = selectedSort.SortFunction(filteredItems).ToList();
            }

            Dispatcher.Invoke(() =>
            {
                Vehicles.Clear();
                foreach (var vehicle in filteredItems)
                {
                    Vehicles.Add(vehicle);
                }

                ListViewExploreCars.SelectedIndex = filteredItems.Any() ? 0 : -1;
            });

        }

        private System.Windows.Threading.DispatcherTimer _searchTimer;
        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchTimer != null)
            {
                _searchTimer.Stop();
            }
            else
            {
                _searchTimer = new System.Windows.Threading.DispatcherTimer();
                _searchTimer.Interval = TimeSpan.FromMilliseconds(300);
                _searchTimer.Tick += (s, args) =>
                {
                    _searchTimer.Stop();
                    UpdateItemsAsync();
                };
            }
            _searchTimer.Start();
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateItemsAsync();

        private void ComboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateItemsAsync();

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxFilter.SelectedIndex = -1;
            ComboBoxSort.SelectedIndex = -1;
            TextBoxSearch.Clear();
            UpdateItemsAsync();
        }

        private async void ButtonRent_Click(object sender, RoutedEventArgs e)
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
            else if (SessionManager.CurrentUser.RoleID == 2)
            {
                MessageBox.Show("Администратор не может оформлять аренды.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ListViewExploreCars.SelectedItem is Vehicle selectedVehicle &&
                DatePickerStart.SelectedDate.HasValue &&
                DatePickerEnd.SelectedDate.HasValue)
            {
                await AsyncOperationHelper.RunWithProgressAsync(async () =>
                {
                    using (var context = new DBEntities())
                    {
                        var booking = new Booking
                        {
                            VehicleID = selectedVehicle.VehicleID,
                            UserID = SessionManager.CurrentUser.UserID,
                            StartDate = DatePickerStart.SelectedDate.Value,
                            EndDate = DatePickerEnd.SelectedDate.Value,
                            TotalCost = rentCost,
                            InsuranceID = (ComboBoxInsurance.SelectedItem as Insurance)?.InsuranceID,
                            StatusID = 1,
                            CreatedAt = DateTime.Now
                        };

                        context.Bookings.Add(booking);
                        await context.SaveChangesAsync();

                        var payment = new Payment
                        {
                            BookingID = booking.BookingID,
                            Amount = rentCost,
                            PaymentMethodID = null,
                            PaymentStatusID = 1,
                            CreatedAt = DateTime.Now
                        };

                        context.Payments.Add(payment);
                        await context.SaveChangesAsync();
                    }
                    return true;
                }, LoadingProgressBar, ButtonRent);

                MessageBox.Show("Бронирование отправлено на подтверждение.", "Заявка отправлена", MessageBoxButton.OK, MessageBoxImage.Information);

                DatePickerStart.SelectedDate = null;
                DatePickerEnd.SelectedDate = null;
                ComboBoxInsurance.SelectedIndex = -1;
                DatePickerEnd.IsEnabled = false;
                ComboBoxInsurance.IsEnabled = false;
                ButtonRent.IsEnabled = false;

                await LoadVehiclesAsync();
            }
        }

        private void AdjustDatesForBlackout()
        {
            foreach (var range in DatePickerStart.BlackoutDates)
            {
                if (DatePickerStart.SelectedDate.HasValue &&
                    DatePickerStart.SelectedDate.Value <= range.End &&
                    DatePickerEnd.SelectedDate.HasValue &&
                    DatePickerEnd.SelectedDate.Value >= range.Start)
                {
                    DatePickerStart.SelectedDate = range.End.AddDays(1);
                    DatePickerEnd.SelectedDate = DatePickerStart.SelectedDate.Value;
                    break;
                }
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

                if (DatePickerEnd.SelectedDate.HasValue && DatePickerStart.SelectedDate > DatePickerEnd.SelectedDate)
                {
                    DatePickerEnd.SelectedDate = DatePickerStart.SelectedDate.Value;
                }

                AdjustDatesForBlackout();

                DatePickerEnd.IsEnabled = true;
                DatePickerEnd.DisplayDateStart = DatePickerStart.SelectedDate.Value;
                DatePickerEnd.DisplayDateEnd = DatePickerStart.SelectedDate.Value.AddMonths(1);
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
                if (DatePickerEnd.SelectedDate < DatePickerStart.SelectedDate)
                {
                    DatePickerEnd.SelectedDate = DatePickerStart.SelectedDate.Value;
                }
                if (DatePickerEnd.SelectedDate > DatePickerStart.SelectedDate.Value.AddMonths(1))
                {
                    DatePickerEnd.SelectedDate = DatePickerStart.SelectedDate.Value.AddMonths(1);
                }

                AdjustDatesForBlackout();

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

        private void ComboBoxInsurance_SelectionChanged(object sender, SelectionChangedEventArgs e) => CalculateRentCost();

        private void CalculateRentCost()
        {
            if (ListViewExploreCars.SelectedItem is Vehicle vehicle && DatePickerEnd.SelectedDate.HasValue && DatePickerStart.SelectedDate.HasValue)
            {
                TimeSpan rentalDuration = DatePickerEnd.SelectedDate.Value.AddDays(1) - DatePickerStart.SelectedDate.Value;
                int numberOfDays = rentalDuration.Days;
                rentCost = vehicle.DailyRate * numberOfDays;

                if (ComboBoxInsurance.SelectedItem is Insurance insurance)
                {
                    rentCost = vehicle.DailyRate * numberOfDays + (Decimal)insurance.InsurancePrice;
                }

                ButtonRent.Content = "Арендовать за " + rentCost.ToString("C");
            }
        }
    }
}