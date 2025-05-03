using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Classes;
using System.Collections.Generic;

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

        public ExploreCarsPage()
        {
            InitializeComponent();
            Vehicles = new ObservableCollection<Vehicle>(DBEntities.GetContext().Vehicles.ToList().Where(x => x.Available == true));
            ListViewExploreCars.ItemsSource = Vehicles;

            ComboBoxFilter.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();

            InitializeSortOptions();

            ComboBoxSort.ItemsSource = sortOptions;

            ListViewExploreCars.SelectedIndex = 0;

            ComboBoxInsurance.ItemsSource = DBEntities.GetContext().Insurances.ToList();

            MainWindow = Application.Current.MainWindow as MainWindow;

            Loaded += ExploreCarsPage_Loaded;

        }

        private void ExploreCarsPage_Loaded(object sender, RoutedEventArgs e)
        {
            DatePickerStart.DisplayDateStart = DateTime.Today.AddDays(1);
            DatePickerStart.DisplayDateEnd = DateTime.Today.AddMonths(1);
           
            foreach (var booking in DBEntities.GetContext().Bookings
                .Where(x => x.BookingStatus.BookingStatus1 == "Confirmed")
                .ToList())
            {
                if (booking.StartDate <= booking.EndDate)
                {
                    DatePickerStart.BlackoutDates.Add(new CalendarDateRange(booking.StartDate, booking.EndDate));
                    DatePickerEnd.BlackoutDates.Add(new CalendarDateRange(booking.StartDate, booking.EndDate));
                }
            }

            LoadingProgressBar.Visibility = Visibility.Collapsed;
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

        private void ListViewExploreCars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = ListViewExploreCars.SelectedItem as Vehicle;
            DBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(X => X.Reload());
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

            ListViewExploreCars.ItemsSource = filteredItems;
            ListViewExploreCars.SelectedIndex = filteredItems.Any() ? 0 : -1;
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateItems();
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateItems();
        }

        private void ComboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateItems();
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxFilter.SelectedIndex = -1;
            ComboBoxSort.SelectedIndex = -1;
            TextBoxSearch.Clear();
            UpdateItems();
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
                if (ListViewExploreCars.SelectedItem is Vehicle selectedVehicle &&
                    DatePickerStart.SelectedDate.HasValue &&
                    DatePickerEnd.SelectedDate.HasValue)
                {
                    try
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

                        DBEntities.GetContext().Bookings.Add(booking);
                        DBEntities.GetContext().SaveChanges();

                        MessageBox.Show("Аренда успешно оформлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        DatePickerStart.SelectedDate = DatePickerStart.DisplayDateStart;
                        DatePickerEnd.SelectedDate = null;
                        ComboBoxInsurance.SelectedIndex = -1;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка при оформлении аренды: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void AdjustDatesForBlackout()
        {
            foreach (var range in DatePickerStart.BlackoutDates)
            {
                if (DatePickerStart.SelectedDate.Value <= range.End &&
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

                if (DatePickerStart.SelectedDate > DatePickerEnd.SelectedDate)
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

        private void ComboBoxInsurance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateRentCost();
        }

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

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(X => X.Reload());
            Vehicles = new ObservableCollection<Vehicle>(DBEntities.GetContext().Vehicles.ToList().Where(x => x.Available == true));
            ListViewExploreCars.ItemsSource = Vehicles;
            UpdateItems();
        }

        private void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            DBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(X => X.Reload());
        }
    }
}