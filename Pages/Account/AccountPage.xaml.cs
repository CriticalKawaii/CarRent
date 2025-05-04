using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Classes;
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using WpfApp.Pages.Account;
using System.Text;
using System.Text.RegularExpressions;
using WpfApp.Pages.Admin;
using System.ComponentModel;
using System.Collections.Generic;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;


namespace WpfApp.Pages
{
    public partial class AccountPage : Page
    {
        public class ConfirmationViewModel : INotifyPropertyChanged
        {
            private Booking _booking;
            private PaymentMethod _selectedPaymentMethod;
            private DateTime? _returnDateInput;
            private decimal? _actualCostInput;

            public Booking Booking
            {
                get => _booking;
                set
                {
                    _booking = value;
                    OnPropertyChanged(nameof(Booking));
                }
            }

            public PaymentMethod SelectedPaymentMethod
            {
                get => _selectedPaymentMethod;
                set
                {
                    _selectedPaymentMethod = value;
                    OnPropertyChanged(nameof(SelectedPaymentMethod));
                }
            }

            public DateTime? ReturnDateInput
            {
                get => _returnDateInput;
                set
                {
                    _returnDateInput = value;
                    OnPropertyChanged(nameof(ReturnDateInput));
                }
            }

            public decimal? ActualCostInput
            {
                get => _actualCostInput;
                set
                {
                    _actualCostInput = value;
                    OnPropertyChanged(nameof(ActualCostInput));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public AccountPage()
        {
            InitializeComponent();
            if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.RoleID != 1)
            {
                AccountPageHeaderIcon.Kind = PackIconKind.Administrator;

                TabItemAdministration.Visibility = Visibility.Visible;
                TabItemReports.Visibility = Visibility.Visible;
                TabItemConfirmations.Visibility = Visibility.Visible;

                TabPersonalData.Visibility = Visibility.Collapsed;
                TabRents.Visibility = Visibility.Collapsed;

                TabItemReports.Focus();
                frameAdmin.Navigate(new AdminDashboardPage());
            }
            DataContext = SessionManager.CurrentUser;
            Loaded += AccountPage_Loaded;
        }

        private void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUser != null)
            {
                var userBookings = DBEntities.GetContext().Bookings
                    .Where(b => b.UserID == SessionManager.CurrentUser.UserID)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                DataGridUserBookings.ItemsSource = userBookings;
                DataGridUserBookings.Visibility = userBookings.Any() ? Visibility.Visible : Visibility.Collapsed;
                TextBlockNoBookings.Visibility = userBookings.Any() ? Visibility.Collapsed : Visibility.Visible;

                if (SessionManager.CurrentUser.RoleID != 1)
                {
                    InitializeReportingSystem();
                    SetupAdminConfirmations();
                }
                else
                {
                    DataContext = SessionManager.CurrentUser;
                }
            }
        }


        private void ButtonReview_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReviewPage((sender as Button).DataContext as Booking));
        }

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$");
        }
        private string GetHash(string password)
        {
            using (var hash = System.Security.Cryptography.SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        private void ButtonCancelBooking_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var booking = button.DataContext as Booking;

            if (booking == null || booking.BookingStatus.BookingStatus1 != "Confirmed")
                return;

            if (booking.StartDate.Date <= DateTime.Today)
            {
                MessageBox.Show("Нельзя отменить бронирование, которое уже началось.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы действительно хотите отменить бронирование?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    booking.StatusID = 5; // Pending Cancellation
                    DBEntities.GetContext().SaveChanges();
                    RefreshUserBookings();
                    MessageBox.Show("Запрос на отмену отправлен администратору.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отмене бронирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ButtonWithdrawCancellation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var booking = button.DataContext as Booking;

            if (booking == null || booking.BookingStatus.BookingStatus1 != "Pending Cancellation")
                return;

            try
            {
                booking.StatusID = 2;
                DBEntities.GetContext().SaveChanges();
                RefreshUserBookings();
                MessageBox.Show("Отмена бронирования отозвана.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отзыве отмены: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonUserConfirmCompletion_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var booking = button.DataContext as Booking;

            if (booking == null || booking.BookingStatus.BookingStatus1 != "Confirmed" || !booking.IsStarted)
                return;

            try
            {
                booking.StatusID = 6; 
                DBEntities.GetContext().SaveChanges();
                RefreshUserBookings();
                MessageBox.Show("Запрос на подтверждение завершения отправлен администратору.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении завершения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshUserBookings()
        {
            if (SessionManager.CurrentUser != null)
            {
                var userBookings = DBEntities.GetContext().Bookings
                    .Where(b => b.UserID == SessionManager.CurrentUser.UserID)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                DataGridUserBookings.ItemsSource = userBookings;
                DataGridUserBookings.Visibility = userBookings.Any() ? Visibility.Visible : Visibility.Collapsed;
                TextBlockNoBookings.Visibility = userBookings.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
        }


        private void ButtonSaveUserData_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            
            if (string.IsNullOrWhiteSpace(TextBoxEmail.Text))
                errors.AppendLine("Введите E-mail!");
            if (string.IsNullOrWhiteSpace(TextBoxFirstName.Text))
                errors.AppendLine("Введите имя!");
            if (string.IsNullOrWhiteSpace(TextBoxLastName.Text))
                errors.AppendLine("Введите фамилию!");

            if (!string.IsNullOrWhiteSpace(TextBoxEmail.Text) && !IsValidEmail(TextBoxEmail.Text))
                errors.AppendLine("Введите корректный E-mail!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var dbUser = DBEntities.GetContext().Users.Find(SessionManager.CurrentUser.UserID);

                if (dbUser == null)
                {
                    MessageBox.Show("Пользователь не найден в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                dbUser.Email = TextBoxEmail.Text;
                dbUser.FirstName = TextBoxFirstName.Text;
                dbUser.LastName = TextBoxLastName.Text;

                if (!string.IsNullOrEmpty(PasswordBoxNewPassword.Password))
                {
                    dbUser.PasswordHash = GetHash(PasswordBoxNewPassword.Password);
                }

                DBEntities.GetContext().SaveChanges();

                SessionManager.SignIn(dbUser);

                DataContext = dbUser;

                MessageBox.Show("Личные данные успешно обновлены", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                PasswordBoxNewPassword.Password = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonSignOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.SignOut();
            NavigationService.Navigate(new SignInPage());
        }

        #region Reporting System

        private ReportType _currentReportType = ReportType.UserActivity;
        private DateTime? _reportStartDate = null;
        private DateTime? _reportEndDate = null;
        private VehicleCategory _selectedCategory = null;

        private enum ReportType
        {
            UserActivity,
            Financial,
            CategoryPopularity,
            VehiclePerformance
        }

        private void InitializeReportingSystem()
        {
            // Initialize Chart
            ChartReport.ChartAreas.Clear();
            ChartReport.Series.Clear();
            ChartReport.Titles.Clear();

            ChartReport.ChartAreas.Add(new ChartArea("MainArea"));
            ChartReport.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            ChartReport.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            ChartReport.ChartAreas[0].AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9);
            ChartReport.ChartAreas[0].AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9);

            // Set default dates
            _reportStartDate = DateTime.Today.AddMonths(-1);
            _reportEndDate = DateTime.Today;

            // Set vehicle categories
            ComboBoxVehicleCategory.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();

            // Set default report period
            ComboBoxReportPeriod.SelectedIndex = 1; // Current Month

            // Set default report type
            ComboBoxReportType.SelectedIndex = 0; // User Activity

            // Generate initial report
            GenerateReport();
        }

        private void ComboBoxReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxReportType.SelectedIndex < 0) return;

            switch (ComboBoxReportType.SelectedIndex)
            {
                case 0:
                    _currentReportType = ReportType.UserActivity;
                    break;
                case 1:
                    _currentReportType = ReportType.Financial;
                    break;
                case 2:
                    _currentReportType = ReportType.CategoryPopularity;
                    break;
                case 3:
                    _currentReportType = ReportType.VehiclePerformance;
                    break;
            }

            // Show/hide category filter based on report type
            ComboBoxVehicleCategory.Visibility = (_currentReportType == ReportType.VehiclePerformance)
                ? Visibility.Visible : Visibility.Collapsed;

            GenerateReport();
        }

        private void ComboBoxReportPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxReportPeriod.SelectedIndex < 0) return;

            bool enableCustomDates = ComboBoxReportPeriod.SelectedIndex == 5; // Custom period
            DatePickerStartDate.IsEnabled = enableCustomDates;
            DatePickerEndDate.IsEnabled = enableCustomDates;

            if (!enableCustomDates)
            {
                // Set predefined date ranges
                switch (ComboBoxReportPeriod.SelectedIndex)
                {
                    case 0: // All time
                        _reportStartDate = new DateTime(2020, 1, 1); // Arbitrary start date
                        _reportEndDate = DateTime.Today;
                        break;
                    case 1: // Current month
                        _reportStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        _reportEndDate = DateTime.Today;
                        break;
                    case 2: // Last 3 months
                        _reportStartDate = DateTime.Today.AddMonths(-3);
                        _reportEndDate = DateTime.Today;
                        break;
                    case 3: // Last 6 months
                        _reportStartDate = DateTime.Today.AddMonths(-6);
                        _reportEndDate = DateTime.Today;
                        break;
                    case 4: // Current year
                        _reportStartDate = new DateTime(DateTime.Today.Year, 1, 1);
                        _reportEndDate = DateTime.Today;
                        break;
                }

                DatePickerStartDate.SelectedDate = _reportStartDate;
                DatePickerEndDate.SelectedDate = _reportEndDate;

                GenerateReport();
            }
        }

        private void DatePickerReportDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerStartDate.SelectedDate.HasValue && DatePickerEndDate.SelectedDate.HasValue)
            {
                _reportStartDate = DatePickerStartDate.SelectedDate.Value;
                _reportEndDate = DatePickerEndDate.SelectedDate.Value;

                // Ensure start date is before end date
                if (_reportStartDate > _reportEndDate)
                {
                    if (sender == DatePickerStartDate)
                    {
                        _reportStartDate = _reportEndDate;
                        DatePickerStartDate.SelectedDate = _reportStartDate;
                    }
                    else
                    {
                        _reportEndDate = _reportStartDate;
                        DatePickerEndDate.SelectedDate = _reportEndDate;
                    }
                }
                GenerateReport();
            }
        }

        private void ReportFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            _selectedCategory = ComboBoxVehicleCategory.SelectedItem as VehicleCategory;
            GenerateReport();
        }

        private void ButtonApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void GenerateReport()
        {
            if (_reportStartDate == null || _reportEndDate == null) return;

            ChartReport.Series.Clear();
            DataGridReportData.Columns.Clear();

            switch (_currentReportType)
            {
                case ReportType.UserActivity:
                    GenerateUserActivityReport();
                    break;
                case ReportType.Financial:
                    GenerateFinancialReport();
                    break;
                case ReportType.CategoryPopularity:
                    GenerateCategoryPopularityReport();
                    break;
                case ReportType.VehiclePerformance:
                    GenerateVehiclePerformanceReport();
                    break;
            }
        }

        private void GenerateUserActivityReport()
        {
            // Set chart title
            ChartReport.Titles.Clear();
            ChartReport.Titles.Add(new Title("Активность пользователей"));
            ChartReport.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

            // Get data
            var bookings = DBEntities.GetContext().Bookings
                .Where(b => b.CreatedAt >= _reportStartDate && b.CreatedAt <= _reportEndDate)
                .ToList();

            // Create series for number of bookings per user
            var seriesBookings = new Series("Количество бронирований");
            seriesBookings.ChartType = SeriesChartType.Column;
            seriesBookings.IsValueShownAsLabel = true;

            // Group by user and count bookings
            var userBookings = bookings
                .GroupBy(b => b.User)
                .Select(g => new { User = g.Key, BookingCount = g.Count() })
                .OrderByDescending(x => x.BookingCount)
                .Take(10) // Top 10 users
                .ToList();

            foreach (var item in userBookings)
            {
                string userName = $"{item.User.FirstName} {item.User.LastName}";
                seriesBookings.Points.AddXY(userName, item.BookingCount);
            }

            ChartReport.Series.Add(seriesBookings);

            // Create DataGrid columns
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Пользователь",
                Binding = new System.Windows.Data.Binding("UserName")
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Бронирований",
                Binding = new System.Windows.Data.Binding("BookingCount")
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Email",
                Binding = new System.Windows.Data.Binding("Email")
            });

            // Add data to DataGrid
            var gridData = userBookings.Select(item => new {
                UserName = $"{item.User.FirstName} {item.User.LastName}",
                BookingCount = item.BookingCount,
                Email = item.User.Email
            }).ToList();

            DataGridReportData.ItemsSource = gridData;
        }

        private void GenerateFinancialReport()
        {
            // Set chart title
            ChartReport.Titles.Clear();
            ChartReport.Titles.Add(new Title("Финансовый отчет"));
            ChartReport.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

            // Get data
            var bookings = DBEntities.GetContext().Bookings
                .Where(b => b.CreatedAt >= _reportStartDate && b.CreatedAt <= _reportEndDate)
                .ToList();

            // Create series for revenue by month
            var seriesRevenue = new Series("Доход");
            seriesRevenue.ChartType = SeriesChartType.Column;
            seriesRevenue.Color = System.Drawing.Color.ForestGreen;
            seriesRevenue.IsValueShownAsLabel = true;

            // Group by month and sum revenue
            var monthlyRevenue = bookings
                .GroupBy(b => new { Month = b.CreatedAt.Value.Month, Year = b.CreatedAt.Value.Year })
                .Select(g => new {
                    Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Revenue = g.Sum(b => b.TotalCost),
                    BookingCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList();

            foreach (var item in monthlyRevenue)
            {
                string periodLabel = item.Period.ToString("MMM yyyy");
                seriesRevenue.Points.AddXY(periodLabel, item.Revenue);
                seriesRevenue.Points.Last().Label = item.Revenue.ToString("C0");
            }

            ChartReport.Series.Add(seriesRevenue);

            // Add booking count series
            var seriesCount = new Series("Количество бронирований");
            seriesCount.ChartType = SeriesChartType.Line;
            seriesCount.Color = System.Drawing.Color.RoyalBlue;
            seriesCount.BorderWidth = 3;
            seriesCount.MarkerStyle = MarkerStyle.Circle;
            seriesCount.MarkerSize = 8;
            seriesCount.YAxisType = AxisType.Secondary;

            // Add secondary Y axis
            ChartReport.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            ChartReport.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;

            foreach (var item in monthlyRevenue)
            {
                string periodLabel = item.Period.ToString("MMM yyyy");
                seriesCount.Points.AddXY(periodLabel, item.BookingCount);
            }

            ChartReport.Series.Add(seriesCount);

            // Create DataGrid columns
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Период",
                Binding = new System.Windows.Data.Binding("Period")
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Доход",
                Binding = new System.Windows.Data.Binding("Revenue") { StringFormat = "{0:C}" }
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Бронирований",
                Binding = new System.Windows.Data.Binding("BookingCount")
            });

            // Add data to DataGrid
            var gridData = monthlyRevenue.Select(item => new {
                Period = item.Period.ToString("MMMM yyyy"),
                Revenue = item.Revenue,
                BookingCount = item.BookingCount
            }).ToList();

            DataGridReportData.ItemsSource = gridData;
        }

        private void GenerateCategoryPopularityReport()
        {
            // Set chart title
            ChartReport.Titles.Clear();
            ChartReport.Titles.Add(new Title("Популярность категорий транспорта"));
            ChartReport.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

            // Get data
            var bookings = DBEntities.GetContext().Bookings
                .Where(b => b.CreatedAt >= _reportStartDate && b.CreatedAt <= _reportEndDate)
                .ToList();

            // Create series for vehicle categories
            var seriesCategories = new Series("Количество бронирований");
            seriesCategories.ChartType = SeriesChartType.Pie;
            seriesCategories.IsValueShownAsLabel = true;

            // Group by category and count bookings
            var categoryBookings = bookings
                .Where(b => b.Vehicle != null && b.Vehicle.VehicleCategory != null)
                .GroupBy(b => b.Vehicle.VehicleCategory)
                .Select(g => new {
                    Category = g.Key,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(b => b.TotalCost)
                })
                .OrderByDescending(x => x.BookingCount)
                .ToList();

            // Configure pie chart
            ChartReport.ChartAreas[0].Area3DStyle.Enable3D = true;
            ChartReport.ChartAreas[0].Area3DStyle.Inclination = 20;

            foreach (var item in categoryBookings)
            {
                string categoryName = item.Category.VehicleCategory1;
                int pointIndex = seriesCategories.Points.AddXY(categoryName, item.BookingCount);
                DataPoint point = seriesCategories.Points[pointIndex];
                point.Label = $"{categoryName}: {item.BookingCount}";

                point.Color = GetRandomColor(item.Category.VehicleCategoryID);
            }

            ChartReport.Series.Add(seriesCategories);

            // Create DataGrid columns
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Категория",
                Binding = new System.Windows.Data.Binding("Category")
            });
            DataGridReportData.Columns.Add(new  System.Windows.Controls.DataGridTextColumn
            {
                Header = "Бронирований",
                Binding = new System.Windows.Data.Binding("BookingCount")
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Доход",
                Binding = new System.Windows.Data.Binding("Revenue") { StringFormat = "{0:C}" }
            });
            DataGridReportData.Columns.Add(new  System.Windows.Controls.DataGridTextColumn
            {
                Header = "Доля (%)",
                Binding = new System.Windows.Data.Binding("Percentage") { StringFormat = "{0:F1}%" }
            });

            // Calculate percentage
            int totalBookings = categoryBookings.Sum(x => x.BookingCount);

            // Add data to DataGrid
            var gridData = categoryBookings.Select(item => new {
                Category = item.Category.VehicleCategory1,
                BookingCount = item.BookingCount,
                Revenue = item.Revenue,
                Percentage = (double)item.BookingCount / totalBookings * 100
            }).ToList();

            DataGridReportData.ItemsSource = gridData;
        }

        private void GenerateVehiclePerformanceReport()
        {
            // Set chart title
            ChartReport.Titles.Clear();
            ChartReport.Titles.Add(new Title("Эффективность транспорта"));
            ChartReport.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

            // Get data with filter for selected category if applicable
            var query = DBEntities.GetContext().Bookings
                .Where(b => b.CreatedAt >= _reportStartDate && b.CreatedAt <= _reportEndDate);

            if (_selectedCategory != null)
            {
                query = query.Where(b => b.Vehicle.VehicleCategoryID == _selectedCategory.VehicleCategoryID);
            }

            var bookings = query.ToList();

            // Create a series for the chart
            var seriesVehicles = new Series("Доход на автомобиль");
            seriesVehicles.ChartType = SeriesChartType.Bar;
            seriesVehicles.IsValueShownAsLabel = true;

            // Group by vehicle and calculate metrics
            var vehicleMetrics = bookings
                .Where(b => b.Vehicle != null)
                .GroupBy(b => b.Vehicle)
                .Select(g => new {
                    Vehicle = g.Key,
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalCost),
                    TotalDays = g.Sum(b => (b.EndDate - b.StartDate).Days),
                    AvgRating = g.Key.AvgRating
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(15) // Top 15 vehicles
                .ToList();

            foreach (var item in vehicleMetrics)
            {
                string vehicleName = $"{item.Vehicle.Make} {item.Vehicle.Model}";
                seriesVehicles.Points.AddXY(vehicleName, item.TotalRevenue);
                seriesVehicles.Points.Last().Label = item.TotalRevenue.ToString("C0");

                // Color based on performance (higher revenue = greener)
                decimal maxRevenue = vehicleMetrics.Max(v => v.TotalRevenue);
                decimal minRevenue = vehicleMetrics.Min(v => v.TotalRevenue);
                decimal range = maxRevenue - minRevenue;

                if (range > 0)
                {
                    // Calculate intensity (0.0 to 1.0)
                    double intensity = (double)((item.TotalRevenue - minRevenue) / range);

                    // Gradient from red to green
                    byte r = (byte)(255 * (1 - intensity));
                    byte g = (byte)(255 * intensity);
                    byte b = 0;

                    seriesVehicles.Points.Last().Color = System.Drawing.Color.FromArgb(r, g, b);
                }
            }

            ChartReport.Series.Add(seriesVehicles);

            // Create DataGrid columns
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Автомобиль",
                Binding = new System.Windows.Data.Binding("VehicleName")
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Бронирований",
                Binding = new System.Windows.Data.Binding("BookingCount")
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Доход",
                Binding = new System.Windows.Data.Binding("Revenue") { StringFormat = "{0:C}" }
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Дней в аренде",
                Binding = new System.Windows.Data.Binding("TotalDays")
            });
            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Рейтинг",
                Binding = new System.Windows.Data.Binding("Rating") { StringFormat = "{0:F1}" }
            });

            // Add data to DataGrid
            var gridData = vehicleMetrics.Select(item => new {
                VehicleName = $"{item.Vehicle.Make} {item.Vehicle.Model} {item.Vehicle.Year}",
                BookingCount = item.BookingCount,
                Revenue = item.TotalRevenue,
                TotalDays = item.TotalDays,
                Rating = item.AvgRating
            }).ToList();

            DataGridReportData.ItemsSource = gridData;
        }

        private System.Drawing.Color GetRandomColor(int seed)
        {
            // Use a consistent method to generate colors based on seed
            var random = new Random(seed);

            // Ensure colors are vivid
            byte r = (byte)random.Next(100, 240);
            byte g = (byte)random.Next(100, 240);
            byte b = (byte)random.Next(100, 240);

            return System.Drawing.Color.FromArgb(r, g, b);
        }

        private void ButtonExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Create Word application
                var wordApp = new Word.Application();
                wordApp.Visible = false;

                // Add new document
                Word.Document document = wordApp.Documents.Add();

                // Add title
                string reportTitle = "Отчет: ";
                switch (_currentReportType)
                {
                    case ReportType.UserActivity:
                        reportTitle += "Активность пользователей";
                        break;
                    case ReportType.Financial:
                        reportTitle += "Финансовый отчет";
                        break;
                    case ReportType.CategoryPopularity:
                        reportTitle += "Популярность категорий транспорта";
                        break;
                    case ReportType.VehiclePerformance:
                        reportTitle += "Эффективность транспорта";
                        break;
                }

                Word.Paragraph titleParagraph = document.Paragraphs.Add();
                titleParagraph.Range.Text = reportTitle;
                titleParagraph.Range.Font.Size = 16;
                titleParagraph.Range.Font.Bold = 1;
                titleParagraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                titleParagraph.Range.InsertParagraphAfter();

                // Add date range
                Word.Paragraph dateParagraph = document.Paragraphs.Add();
                dateParagraph.Range.Text = $"Период: {_reportStartDate:dd.MM.yyyy} - {_reportEndDate:dd.MM.yyyy}";
                dateParagraph.Range.Font.Size = 12;
                dateParagraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                dateParagraph.Range.InsertParagraphAfter();

                // Add chart image
                ChartReport.SaveImage(System.IO.Path.GetTempPath() + "tempChart.png", System.Drawing.Imaging.ImageFormat.Png);
                Word.Paragraph chartParagraph = document.Paragraphs.Add();
                Word.Range chartRange = chartParagraph.Range;
                chartRange.InlineShapes.AddPicture(System.IO.Path.GetTempPath() + "tempChart.png");
                chartRange.InsertParagraphAfter();

                // Add table with data
                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;

                // Get data from DataGrid
                var dataItems = DataGridReportData.ItemsSource as System.Collections.IEnumerable;
                if (dataItems != null)
                {
                    var itemsList = dataItems.Cast<object>().ToList();
                    if (itemsList.Any())
                    {
                        var properties = itemsList[0].GetType().GetProperties();

                        // Create table
                        Word.Table dataTable = document.Tables.Add(tableRange, itemsList.Count + 1, properties.Length);
                        dataTable.Borders.InsideLineStyle = dataTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                        // Add headers
                        for (int col = 0; col < properties.Length; col++)
                        {
                            dataTable.Cell(1, col + 1).Range.Text = DataGridReportData.Columns[col].Header.ToString();
                            dataTable.Cell(1, col + 1).Range.Font.Bold = 1;
                        }

                        // Add data
                        for (int row = 0; row < itemsList.Count; row++)
                        {
                            for (int col = 0; col < properties.Length; col++)
                            {
                                var value = properties[col].GetValue(itemsList[row]);
                                dataTable.Cell(row + 2, col + 1).Range.Text = value?.ToString() ?? "";
                            }
                        }

                        // Format table
                        dataTable.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        dataTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    }
                }

                // Add summary paragraph
                Word.Paragraph summaryParagraph = document.Paragraphs.Add();
                summaryParagraph.Range.Text = "Отчет составлен: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                summaryParagraph.Range.Font.Italic = 1;
                summaryParagraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

                // Save document
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Word Document (*.docx)|*.docx",
                    DefaultExt = ".docx",
                    FileName = $"Отчет_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    document.SaveAs(saveFileDialog.FileName);
                    document.Close();
                    wordApp.Quit();

                    MessageBox.Show("Отчет успешно экспортирован в Word", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    document.Close(false);
                    wordApp.Quit();
                }

                // Delete temporary files
                try { System.IO.File.Delete(System.IO.Path.GetTempPath() + "tempChart.png"); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Create Excel application
                var excelApp = new Excel.Application();
                excelApp.Visible = false;

                // Add new workbook
                Excel.Workbook workbook = excelApp.Workbooks.Add();
                Excel.Worksheet worksheet = workbook.Sheets[1];

                // Set title
                string reportTitle = "Отчет: ";
                switch (_currentReportType)
                {
                    case ReportType.UserActivity:
                        reportTitle += "Активность пользователей";
                        break;
                    case ReportType.Financial:
                        reportTitle += "Финансовый отчет";
                        break;
                    case ReportType.CategoryPopularity:
                        reportTitle += "Популярность категорий транспорта";
                        break;
                    case ReportType.VehiclePerformance:
                        reportTitle += "Эффективность транспорта";
                        break;
                }

                worksheet.Cells[1, 1] = reportTitle;
                Excel.Range titleRange = worksheet.Range["A1:E1"];
                titleRange.Merge();
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 14;
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Add date range
                worksheet.Cells[2, 1] = $"Период: {_reportStartDate:dd.MM.yyyy} - {_reportEndDate:dd.MM.yyyy}";
                Excel.Range dateRange = worksheet.Range["A2:E2"];
                dateRange.Merge();
                dateRange.Font.Italic = true;
                dateRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Insert chart image
                ChartReport.SaveImage(System.IO.Path.GetTempPath() + "tempChart.png", System.Drawing.Imaging.ImageFormat.Png);
                Excel.Range chartRange = worksheet.Range["A4:E20"];
                worksheet.Shapes.AddPicture(
                    System.IO.Path.GetTempPath() + "tempChart.png",
                    Microsoft.Office.Core.MsoTriState.msoFalse,
                    Microsoft.Office.Core.MsoTriState.msoTrue,
                    chartRange.Left, chartRange.Top, chartRange.Width, chartRange.Height);

                // Get data from DataGrid
                var dataItems = DataGridReportData.ItemsSource as System.Collections.IEnumerable;
                if (dataItems != null)
                {
                    var itemsList = dataItems.Cast<object>().ToList();
                    if (itemsList.Any())
                    {
                        var properties = itemsList[0].GetType().GetProperties();

                        // Start at row 22 (below chart)
                        int currentRow = 22;

                        // Add headers
                        for (int col = 0; col < properties.Length; col++)
                        {
                            worksheet.Cells[currentRow, col + 1] = DataGridReportData.Columns[col].Header.ToString();
                            worksheet.Cells[currentRow, col + 1].Font.Bold = true;
                        }

                        // Format header row
                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, properties.Length]];
                        headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                        headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                        // Add data
                        currentRow++;
                        for (int row = 0; row < itemsList.Count; row++)
                        {
                            for (int col = 0; col < properties.Length; col++)
                            {
                                var value = properties[col].GetValue(itemsList[row]);
                                worksheet.Cells[currentRow + row, col + 1] = value;

                                // Set currency format for financial values
                                try
                                {
                                    if (value != null &&
                                        ((value is decimal || value is double) &&
                                        (properties[col].Name.Contains("Revenue") || properties[col].Name.Contains("Cost"))))
                                    {
                                        Excel.Range cell = worksheet.Cells[currentRow + row, col + 1];
                                        cell.NumberFormat = "_-* #,##0.00₽_-;-* #,##0.00₽_-;_-* \"-\"??₽_-;_-@_-";
                                    }
                                }
                                catch
                                {
                                    // Silently continue if number format can't be set
                                }
                            }
                        }

                        // Format data range
                        Excel.Range dataRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow + itemsList.Count - 1, properties.Length]];
                        dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        dataRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                        // Autofit columns
                        worksheet.UsedRange.Columns.AutoFit();
                    }
                }

                // Save workbook
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    DefaultExt = ".xlsx",
                    FileName = $"Отчет_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    workbook.SaveAs(saveFileDialog.FileName);
                    workbook.Close();
                    excelApp.Quit();

                    MessageBox.Show("Отчет успешно экспортирован в Excel", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    workbook.Close(false);
                    excelApp.Quit();
                }

                // Delete temporary files
                try { System.IO.File.Delete(System.IO.Path.GetTempPath() + "tempChart.png"); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion


        #region Admin

        private void SetupAdminConfirmations()
        {
            PaymentMethods = DBEntities.GetContext().PaymentMethods.ToList();
            DataContext = this;

            LoadPendingPayments();
            LoadPendingCancellations();
            LoadPendingCompletions();
        }

        public List<PaymentMethod> PaymentMethods { get; set; }

        private void ToggleButtonConfirmations_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;

            if (GridPendingPayments != null)
                GridPendingPayments.Visibility = Visibility.Collapsed;
            if (GridPendingCancellations != null)
                GridPendingCancellations.Visibility = Visibility.Collapsed;
            if (GridPendingCompletions != null)
                GridPendingCompletions.Visibility = Visibility.Collapsed;

            if (radioButton == ToggleButtonPendingPayments && GridPendingPayments != null)
            {
                GridPendingPayments.Visibility = Visibility.Visible;
                LoadPendingPayments();
            }
            else if (radioButton == ToggleButtonPendingCancellations && GridPendingCancellations != null)
            {
                GridPendingCancellations.Visibility = Visibility.Visible;
                LoadPendingCancellations();
            }
            else if (radioButton == ToggleButtonPendingCompletions && GridPendingCompletions != null)
            {
                GridPendingCompletions.Visibility = Visibility.Visible;
                LoadPendingCompletions();
            }
        }


        private void LoadPendingPayments()
        {
            var pendingBookings = DBEntities.GetContext().Bookings
                .Where(b => b.StatusID == 1) // Pending status
                .ToList();

            var viewModels = pendingBookings.Select(booking => new ConfirmationViewModel
            {
                Booking = booking,
                ActualCostInput = booking.TotalCost
            }).ToList();

            DataGridPendingPayments.ItemsSource = viewModels;
        }

        private void LoadPendingCancellations()
        {
            var pendingCancellations = DBEntities.GetContext().Bookings
                .Where(b => b.StatusID == 5) // Pending Cancellation
                .ToList();

            var viewModels = pendingCancellations.Select(booking => new ConfirmationViewModel
            {
                Booking = booking
            }).ToList();

            DataGridPendingCancellations.ItemsSource = viewModels;
        }

        private void LoadPendingCompletions()
        {
            var pendingCompletions = DBEntities.GetContext().Bookings
                .Where(b => b.StatusID == 6) // Pending Completion
                .ToList();

            var viewModels = pendingCompletions.Select(booking => new ConfirmationViewModel
            {
                Booking = booking,
                ReturnDateInput = DateTime.Today,
                ActualCostInput = booking.TotalCost
            }).ToList();

            DataGridPendingCompletions.ItemsSource = viewModels;
        }

        // Payment confirmation methods
        private void ButtonConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null || viewModel.SelectedPaymentMethod == null)
            {
                MessageBox.Show("Выберите способ оплаты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var booking = viewModel.Booking;
                booking.StatusID = 2; // Confirmed

                // Update payment
                var payment = booking.Payments.FirstOrDefault();
                if (payment != null)
                {
                    payment.PaymentStatusID = 2; // Completed
                    payment.PaymentMethodID = viewModel.SelectedPaymentMethod.PaymentMethodID;
                }

                DBEntities.GetContext().SaveChanges();
                LoadPendingPayments();
                MessageBox.Show("Оплата подтверждена, бронирование активировано.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении оплаты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCancelPayment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null)
                return;

            try
            {
                var booking = viewModel.Booking;
                booking.StatusID = 4; // Cancelled

                // Update payment
                var payment = booking.Payments.FirstOrDefault();
                if (payment != null)
                {
                    payment.PaymentStatusID = 3; // Cancelled
                }

                DBEntities.GetContext().SaveChanges();
                LoadPendingPayments();
                MessageBox.Show("Оплата отменена, бронирование отклонено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене оплаты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Cancellation confirmation methods
        private void ButtonConfirmCancellation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null)
                return;

            try
            {
                var booking = viewModel.Booking;
                booking.StatusID = 4; // Cancelled

                DBEntities.GetContext().SaveChanges();
                LoadPendingCancellations();
                MessageBox.Show("Отмена подтверждена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении отмены: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonDenyCancellation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null)
                return;

            try
            {
                var booking = viewModel.Booking;
                booking.StatusID = 2; // Confirmed

                DBEntities.GetContext().SaveChanges();
                LoadPendingCancellations();
                MessageBox.Show("Отмена отклонена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отклонении отмены: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Completion confirmation method
        private void ButtonConfirmCompletion_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null || !viewModel.ReturnDateInput.HasValue || !viewModel.ActualCostInput.HasValue)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var booking = viewModel.Booking;
                booking.StatusID = 3; // Completed
                booking.ReturnDate = viewModel.ReturnDateInput.Value;
                booking.ActualCost = viewModel.ActualCostInput.Value;

                DBEntities.GetContext().SaveChanges();
                LoadPendingCompletions();
                MessageBox.Show("Завершение подтверждено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении завершения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
