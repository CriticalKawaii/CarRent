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
using System.Threading.Tasks;
using System.Data.Entity;
using System.IO;
using System.Drawing;

namespace WpfApp.Pages
{
    public partial class AccountPage : Page
    {
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

        public List<PaymentMethod> PaymentMethods { get; set; }

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

                TabItemConfirmations.Focus();
                frameAdmin.Navigate(new AdminDashboardPage());
            }
            
            Loaded += AccountPage_Loaded;
        }

        private async void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            
            try
            {
                await LoadAccountDataAsync();
            }
            finally
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }
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

        private async Task LoadAccountDataAsync()
        {
            if (SessionManager.CurrentUser != null)
            {
                try
                {
                    await LoadUserBookingsAsync();

                    if (SessionManager.CurrentUser.RoleID != 1)
                    {
                        await LoadAdminDataAsync();
                    }
                    else
                    {
                        DataContext = SessionManager.CurrentUser;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading account data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task LoadUserBookingsAsync()
        {
            using (var context = new DBEntities())
            {
                var bookings = await context.Bookings
                    .Include(b => b.BookingStatus)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Vehicle.VehicleImages)
                    .Where(b => b.UserID == SessionManager.CurrentUser.UserID)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                foreach (var booking in bookings)
                {
                    if (booking.Vehicle != null)
                    {
                        try
                        {
                            await booking.Vehicle.GetImageSourceAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading vehicle image: {ex.Message}");
                        }
                    }
                }

                DataGridUserBookings.ItemsSource = bookings;
                DataGridUserBookings.Visibility = bookings.Any() ? Visibility.Visible : Visibility.Collapsed;
                TextBlockNoBookings.Visibility = bookings.Any() ? Visibility.Collapsed : Visibility.Visible;
            }

        }

        private async Task RefreshUserBookingsAsync()
        {
            try
            {
                await LoadUserBookingsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAdminDataAsync()
        {
            using (var context = new DBEntities())
            {
                await InitializeReportingSystemAsync(context);

                await SetupAdminConfirmationsAsync(context);
            }
        }

        private async Task InitializeReportingSystemAsync(DBEntities context)
        {
            ChartReport.ChartAreas.Clear();
            ChartReport.Series.Clear();
            ChartReport.Titles.Clear();

            ChartReport.ChartAreas.Add(new ChartArea("MainArea"));
            ChartReport.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            ChartReport.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            ChartReport.ChartAreas[0].AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9);
            ChartReport.ChartAreas[0].AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9);

            _reportStartDate = DateTime.Today.AddMonths(-1);
            _reportEndDate = DateTime.Today;

            var vehicleCategories = await context.VehicleCategories.ToListAsync();
            ComboBoxVehicleCategory.ItemsSource = vehicleCategories;

            ComboBoxReportPeriod.SelectedIndex = 1; // этот месяц

            ComboBoxReportType.SelectedIndex = 0; // отчет активность пользователей

            
            GenerateReport();
        }

        private async void ButtonReview_Click(object sender, RoutedEventArgs e)
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

        private async void ButtonCancelBooking_Click(object sender, RoutedEventArgs e)
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
                await AsyncOperationHelper.RunWithProgressAsync(async () => {
                    using (var context = new DBEntities())
                    {
                        var dbBooking = await context.Bookings.FindAsync(booking.BookingID);
                        if (dbBooking != null)
                        {
                            dbBooking.StatusID = 5; // Pending Cancellation
                            await context.SaveChangesWithRetryAsync();
                        }
                    }
                    await RefreshUserBookingsAsync();
                    return true;
                }, LoadingProgressBar, button);
                
                MessageBox.Show("Запрос на отмену отправлен администратору.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private async void ButtonWithdrawCancellation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var booking = button.DataContext as Booking;

            if (booking == null || booking.BookingStatus.BookingStatus1 != "Pending Cancellation")
                return;

            await AsyncOperationHelper.RunWithProgressAsync(async () => {
                using (var context = new DBEntities())
                {
                    var dbBooking = await context.Bookings.FindAsync(booking.BookingID);
                    if (dbBooking != null)
                    {
                        dbBooking.StatusID = 2; // Confirmed
                        await context.SaveChangesWithRetryAsync();
                    }
                }
                await RefreshUserBookingsAsync();
                return true;
            }, LoadingProgressBar, button);
            
            MessageBox.Show("Отмена бронирования отозвана.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ButtonUserConfirmCompletion_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var booking = button.DataContext as Booking;

            if (booking == null || booking.BookingStatus.BookingStatus1 != "Confirmed" || !booking.IsStarted)
                return;

            await AsyncOperationHelper.RunWithProgressAsync(async () => {
                using (var context = new DBEntities())
                {
                    var dbBooking = await context.Bookings.FindAsync(booking.BookingID);
                    if (dbBooking != null)
                    {
                        dbBooking.StatusID = 6; // Pending Completion
                        await context.SaveChangesWithRetryAsync();
                    }
                }
                await RefreshUserBookingsAsync();
                return true;
            }, LoadingProgressBar, button);
            
            MessageBox.Show("Запрос на подтверждение завершения отправлен администратору.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ButtonSaveUserData_Click(object sender, RoutedEventArgs e)
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
            
            await AsyncOperationHelper.RunWithProgressAsync(async () => {
                using (var context = new DBEntities())
                {
                    var dbUser = await context.Users.FindAsync(SessionManager.CurrentUser.UserID);

                    if (dbUser == null)
                    {
                        MessageBox.Show("Пользователь не найден в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    dbUser.Email = TextBoxEmail.Text;
                    dbUser.FirstName = TextBoxFirstName.Text;
                    dbUser.LastName = TextBoxLastName.Text;

                    if (!string.IsNullOrEmpty(PasswordBoxNewPassword.Password))
                    {
                        dbUser.PasswordHash = GetHash(PasswordBoxNewPassword.Password);
                    }

                    await context.SaveChangesWithRetryAsync();

                    SessionManager.SignIn(dbUser);

                    Dispatcher.Invoke(() => {
                        DataContext = dbUser;
                        PasswordBoxNewPassword.Password = string.Empty;
                    });

                    return true;
                }
            }, LoadingProgressBar, ButtonSaveUserData);
            
            MessageBox.Show("Личные данные успешно обновлены", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonSignOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.SignOut();
            NavigationService.Navigate(new SignInPage());
        }

        #region Reporting System


        /*var area = ChartReport.ChartAreas[0];
        area.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
                area.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
                area.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9);
                area.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9);
                area.Area3DStyle.Enable3D = false; // Reset 3D effect*/
        private void GenerateReport()
        {
            if (_reportStartDate == null || _reportEndDate == null) return;

            ChartReport.Series.Clear();
            DataGridReportData.Columns.Clear();
            ChartReport.ChartAreas[0].Area3DStyle.Enable3D = false;

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



        private async void ComboBoxReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            ComboBoxVehicleCategory.Visibility = (_currentReportType == ReportType.VehiclePerformance)
                ? Visibility.Visible : Visibility.Collapsed;

            GenerateReport();
        }

        private async void ComboBoxReportPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxReportPeriod.SelectedIndex < 0) return;

            bool enableCustomDates = ComboBoxReportPeriod.SelectedIndex == 5;
            DatePickerStartDate.IsEnabled = enableCustomDates;
            DatePickerEndDate.IsEnabled = enableCustomDates;

            if (!enableCustomDates)
            {
                switch (ComboBoxReportPeriod.SelectedIndex)
                {
                    case 0: // все время
                        _reportStartDate = new DateTime(2020, 1, 1);
                        _reportEndDate = DateTime.Today;
                        break;
                    case 1: // этот месяц
                        _reportStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        _reportEndDate = DateTime.Today;
                        break;
                    case 2: // последние 3 месяца
                        _reportStartDate = DateTime.Today.AddMonths(-3);
                        _reportEndDate = DateTime.Today;
                        break;
                    case 3: // последние 6 месяцев
                        _reportStartDate = DateTime.Today.AddMonths(-6);
                        _reportEndDate = DateTime.Today;
                        break;
                    case 4: // этот год
                        _reportStartDate = new DateTime(DateTime.Today.Year, 1, 1);
                        _reportEndDate = DateTime.Today;
                        break;
                }

                DatePickerStartDate.SelectedDate = _reportStartDate;
                DatePickerEndDate.SelectedDate = _reportEndDate;

                GenerateReport();
            }
        }

        private async void DatePickerReportDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerStartDate.SelectedDate.HasValue && DatePickerEndDate.SelectedDate.HasValue)
            {
                _reportStartDate = DatePickerStartDate.SelectedDate.Value;
                _reportEndDate = DatePickerEndDate.SelectedDate.Value;

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

        private async void ReportFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            _selectedCategory = ComboBoxVehicleCategory.SelectedItem as VehicleCategory;
            GenerateReport();
        }

        private void GenerateUserActivityReport()
        {
            ChartReport.Titles.Clear();
            ChartReport.Titles.Add(new Title("Активность пользователей"));
            ChartReport.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

            var bookings = DBEntities.GetContext().Bookings
                .Where(b => b.CreatedAt >= _reportStartDate && b.CreatedAt <= _reportEndDate)
                .ToList();

            var seriesBookings = new Series("Количество бронирований");
            seriesBookings.ChartType = SeriesChartType.Column;
            seriesBookings.IsValueShownAsLabel = true;

            var userBookings = bookings
                .GroupBy(b => b.User)
                .Select(g => new { User = g.Key, BookingCount = g.Count() })
                .OrderByDescending(x => x.BookingCount)
                .Take(10)
                .ToList();

            foreach (var item in userBookings)
            {
                string userName = $"{item.User.FirstName} {item.User.LastName}";
                seriesBookings.Points.AddXY(userName, item.BookingCount);
            }

            ChartReport.Series.Add(seriesBookings);

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

            var gridData = userBookings.Select(item => new {
                UserName = $"{item.User.FirstName} {item.User.LastName}",
                BookingCount = item.BookingCount,
                Email = item.User.Email
            }).ToList();

            DataGridReportData.ItemsSource = gridData;
        }

        private void GenerateFinancialReport()
        {
            ChartReport.Titles.Clear();
            ChartReport.Titles.Add(new Title("Финансовый отчет"));
            ChartReport.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

            var bookings = DBEntities.GetContext().Bookings
                .Where(b => b.CreatedAt >= _reportStartDate && b.CreatedAt <= _reportEndDate)
                .ToList();

            var seriesRevenue = new Series("Доход");
            seriesRevenue.ChartType = SeriesChartType.Column;
            seriesRevenue.Color = System.Drawing.Color.ForestGreen;
            seriesRevenue.IsValueShownAsLabel = true;

            var monthlyRevenue = bookings
                .GroupBy(b => new { Month = b.CreatedAt.Month, Year = b.CreatedAt.Year })
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

            var seriesCount = new Series("Количество бронирований");
            seriesCount.ChartType = SeriesChartType.Line;
            seriesCount.Color = System.Drawing.Color.RoyalBlue;
            seriesCount.BorderWidth = 3;
            seriesCount.MarkerStyle = MarkerStyle.Circle;
            seriesCount.MarkerSize = 8;
            seriesCount.YAxisType = AxisType.Secondary;

            ChartReport.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            ChartReport.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;

            foreach (var item in monthlyRevenue)
            {
                string periodLabel = item.Period.ToString("MMM yyyy");
                seriesCount.Points.AddXY(periodLabel, item.BookingCount);
            }

            ChartReport.Series.Add(seriesCount);

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

            var gridData = monthlyRevenue.Select(item => new {
                Period = item.Period.ToString("MMMM yyyy"),
                Revenue = item.Revenue,
                BookingCount = item.BookingCount
            }).ToList();

            DataGridReportData.ItemsSource = gridData;
        }

        private void GenerateCategoryPopularityReport()
        {
            ChartReport.Titles.Clear();
            ChartReport.Titles.Add(new Title("Популярность категорий транспорта"));
            ChartReport.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

            var bookings = DBEntities.GetContext().Bookings
                .Where(b => b.CreatedAt >= _reportStartDate && b.CreatedAt <= _reportEndDate)
                .ToList();

            var seriesCategories = new Series("Количество бронирований");
            seriesCategories.ChartType = SeriesChartType.Pie;
            seriesCategories.IsValueShownAsLabel = true;

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

            DataGridReportData.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Категория",
                Binding = new System.Windows.Data.Binding("Category")
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
                Header = "Доля (%)",
                Binding = new System.Windows.Data.Binding("Percentage") { StringFormat = "{0:F1}%" }
            });

            int totalBookings = categoryBookings.Sum(x => x.BookingCount);

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
            ChartReport.Titles.Clear();
            ChartReport.Titles.Add(new Title("Эффективность транспорта"));
            ChartReport.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

            var query = DBEntities.GetContext().Bookings
                .Where(b => b.CreatedAt >= _reportStartDate && b.CreatedAt <= _reportEndDate);

            if (_selectedCategory != null)
            {
                query = query.Where(b => b.Vehicle.VehicleCategoryID == _selectedCategory.VehicleCategoryID);
            }

            var bookings = query.ToList();

            var seriesVehicles = new Series("Доход на автомобиль");
            seriesVehicles.ChartType = SeriesChartType.Bar;
            seriesVehicles.IsValueShownAsLabel = true;

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
                .Take(15) 
                .ToList();

            foreach (var item in vehicleMetrics)
            {
                string vehicleName = $"{item.Vehicle.Make} {item.Vehicle.Model}";
                seriesVehicles.Points.AddXY(vehicleName, item.TotalRevenue);
                seriesVehicles.Points.Last().Label = item.TotalRevenue.ToString("C0");

                decimal maxRevenue = vehicleMetrics.Max(v => v.TotalRevenue);
                decimal minRevenue = vehicleMetrics.Min(v => v.TotalRevenue);
                decimal range = maxRevenue - minRevenue;

                if (range > 0)
                {
                    double intensity = (double)((item.TotalRevenue - minRevenue) / range);

                    byte r = (byte)(255 * (1 - intensity));
                    byte g = (byte)(255 * intensity);
                    byte b = 0;

                    seriesVehicles.Points.Last().Color = System.Drawing.Color.FromArgb(r, g, b);
                }
            }

            ChartReport.Series.Add(seriesVehicles);

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
            var random = new Random(seed);

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

                var wordApp = new Word.Application();
                wordApp.Visible = false;

                Word.Document document = wordApp.Documents.Add();

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

                Word.Paragraph dateParagraph = document.Paragraphs.Add();
                dateParagraph.Range.Text = $"Период: {_reportStartDate:dd.MM.yyyy} - {_reportEndDate:dd.MM.yyyy}";
                dateParagraph.Range.Font.Size = 12;
                dateParagraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                dateParagraph.Range.InsertParagraphAfter();

                ChartReport.SaveImage(System.IO.Path.GetTempPath() + "tempChart.png", System.Drawing.Imaging.ImageFormat.Png);
                Word.Paragraph chartParagraph = document.Paragraphs.Add();
                Word.Range chartRange = chartParagraph.Range;
                chartRange.InlineShapes.AddPicture(System.IO.Path.GetTempPath() + "tempChart.png");
                chartRange.InsertParagraphAfter();

                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;

                var dataItems = DataGridReportData.ItemsSource as System.Collections.IEnumerable;
                if (dataItems != null)
                {
                    var itemsList = dataItems.Cast<object>().ToList();
                    if (itemsList.Any())
                    {
                        var properties = itemsList[0].GetType().GetProperties();

                        Word.Table dataTable = document.Tables.Add(tableRange, itemsList.Count + 1, properties.Length);
                        dataTable.Borders.InsideLineStyle = dataTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                        for (int col = 0; col < properties.Length; col++)
                        {
                            dataTable.Cell(1, col + 1).Range.Text = DataGridReportData.Columns[col].Header.ToString();
                            dataTable.Cell(1, col + 1).Range.Font.Bold = 1;
                        }

                        for (int row = 0; row < itemsList.Count; row++)
                        {
                            for (int col = 0; col < properties.Length; col++)
                            {
                                var value = properties[col].GetValue(itemsList[row]);
                                dataTable.Cell(row + 2, col + 1).Range.Text = value?.ToString() ?? "";
                            }
                        }

                        dataTable.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        dataTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    }
                }

                Word.Paragraph summaryParagraph = document.Paragraphs.Add();
                summaryParagraph.Range.Text = "Отчет составлен: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                summaryParagraph.Range.Font.Italic = 1;
                summaryParagraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

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

                var excelApp = new Excel.Application();
                excelApp.Visible = false;

                Excel.Workbook workbook = excelApp.Workbooks.Add();
                Excel.Worksheet worksheet = workbook.Sheets[1];

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

                worksheet.Cells[2, 1] = $"Период: {_reportStartDate:dd.MM.yyyy} - {_reportEndDate:dd.MM.yyyy}";
                Excel.Range dateRange = worksheet.Range["A2:E2"];
                dateRange.Merge();
                dateRange.Font.Italic = true;
                dateRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                ChartReport.SaveImage(System.IO.Path.GetTempPath() + "tempChart.png", System.Drawing.Imaging.ImageFormat.Png);
                Excel.Range chartRange = worksheet.Range["A4:E20"];
                worksheet.Shapes.AddPicture(
                    System.IO.Path.GetTempPath() + "tempChart.png",
                    Microsoft.Office.Core.MsoTriState.msoFalse,
                    Microsoft.Office.Core.MsoTriState.msoTrue,
                    chartRange.Left, chartRange.Top, chartRange.Width, chartRange.Height);

                var dataItems = DataGridReportData.ItemsSource as System.Collections.IEnumerable;
                if (dataItems != null)
                {
                    var itemsList = dataItems.Cast<object>().ToList();
                    if (itemsList.Any())
                    {
                        var properties = itemsList[0].GetType().GetProperties();

                        int currentRow = 22;

                        for (int col = 0; col < properties.Length; col++)
                        {
                            worksheet.Cells[currentRow, col + 1] = DataGridReportData.Columns[col].Header.ToString();
                            worksheet.Cells[currentRow, col + 1].Font.Bold = true;
                        }

                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, properties.Length]];
                        headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                        headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                        currentRow++;
                        for (int row = 0; row < itemsList.Count; row++)
                        {
                            for (int col = 0; col < properties.Length; col++)
                            {
                                var value = properties[col].GetValue(itemsList[row]);
                                worksheet.Cells[currentRow + row, col + 1] = value;

                                try
                                {
                                    if (value != null &&
                                        ((value is decimal || value is double) &&
                                        (properties[col].Name.Contains("Revenue") || properties[col].Name.Contains("Cost"))))
                                    {
                                        Excel.Range cell = worksheet.Cells[currentRow + row, col + 1];
                                        cell.NumberFormat = "_-* #,##0.00₽_-;-* #,##0.00₽_-;_-* \"-\"??₽_-;_-@_-";
                                    }
                                }catch{}
                            }
                        }

                        Excel.Range dataRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow + itemsList.Count - 1, properties.Length]];
                        dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        dataRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                        worksheet.UsedRange.Columns.AutoFit();
                    }
                }

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

        private async Task SetupAdminConfirmationsAsync(DBEntities context)
        {
            PaymentMethods = await context.PaymentMethods.ToListAsync();
            DataContext = this;

            await LoadPendingPaymentsAsync(context);
            await LoadPendingCancellationsAsync(context);
            await LoadPendingCompletionsAsync(context);
        }

        private async Task LoadPendingPaymentsAsync(DBEntities context)
        {
            var pendingBookings = await context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Where(b => b.StatusID == 1) // Pending status
                .ToListAsync();

            var viewModels = pendingBookings.Select(booking => new ConfirmationViewModel
            {
                Booking = booking,
                ActualCostInput = booking.TotalCost
            }).ToList();

            DataGridPendingPayments.ItemsSource = viewModels;
        }

        private async Task LoadPendingCancellationsAsync(DBEntities context)
        {
            var pendingCancellations = await context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Where(b => b.StatusID == 5) // Pending Cancellation
                .ToListAsync();

            var viewModels = pendingCancellations.Select(booking => new ConfirmationViewModel
            {
                Booking = booking
            }).ToList();

            DataGridPendingCancellations.ItemsSource = viewModels;
        }

        private async Task LoadPendingCompletionsAsync(DBEntities context)
        {
            var pendingCompletions = await context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Where(b => b.StatusID == 6) 
                .ToListAsync();

            var viewModels = pendingCompletions.Select(booking => new ConfirmationViewModel
            {
                Booking = booking,
                ReturnDateInput = DateTime.Today,
                ActualCostInput = booking.TotalCost
            }).ToList();

            DataGridPendingCompletions.ItemsSource = viewModels;
        }

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
                 LoadPendingPaymentsAsync(DBEntities.GetContext());
            }
            else if (radioButton == ToggleButtonPendingCancellations && GridPendingCancellations != null)
            {
                GridPendingCancellations.Visibility = Visibility.Visible;
                LoadPendingCancellationsAsync(DBEntities.GetContext());
            }
            else if (radioButton == ToggleButtonPendingCompletions && GridPendingCompletions != null)
            {
                GridPendingCompletions.Visibility = Visibility.Visible;
                LoadPendingCompletionsAsync(DBEntities.GetContext());
            }
        }

        private async void ButtonConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null || viewModel.SelectedPaymentMethod == null)
            {
                MessageBox.Show("Выберите способ оплаты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AsyncOperationHelper.RunWithProgressAsync(async () => {
                using (var context = new DBEntities())
                {
                    var booking = await context.Bookings
                        .Include(b => b.Payments)
                        .FirstOrDefaultAsync(b => b.BookingID == viewModel.Booking.BookingID);
                    
                    if (booking != null)
                    {
                        booking.StatusID = 2; // Confirmed

                        var payment = booking.Payments.FirstOrDefault();
                        if (payment != null)
                        {
                            payment.PaymentStatusID = 2; // Completed
                            payment.PaymentMethodID = viewModel.SelectedPaymentMethod.PaymentMethodID;
                        }

                        await context.SaveChangesWithRetryAsync();
                        await LoadPendingPaymentsAsync(context);
                    }
                }
                return true;
            }, LoadingProgressBar, button);
            
            MessageBox.Show("Оплата подтверждена, бронирование активировано.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ButtonCancelPayment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null)
                return;

            await AsyncOperationHelper.RunWithProgressAsync(async () => {
                using (var context = new DBEntities())
                {
                    var booking = await context.Bookings
                        .Include(b => b.Payments)
                        .FirstOrDefaultAsync(b => b.BookingID == viewModel.Booking.BookingID);
                    
                    if (booking != null)
                    {
                        booking.StatusID = 4; 

                        var payment = booking.Payments.FirstOrDefault();
                        if (payment != null)
                        {
                            payment.PaymentStatusID = 3; 
                        }

                        await context.SaveChangesWithRetryAsync();
                        await LoadPendingPaymentsAsync(context);
                    }
                }
                return true;
            }, LoadingProgressBar, button);
            
            MessageBox.Show("Оплата отменена, бронирование отклонено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ButtonConfirmCancellation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null)
                return;

            await AsyncOperationHelper.RunWithProgressAsync(async () => {
                using (var context = new DBEntities())
                {
                    var booking = await context.Bookings.FindAsync(viewModel.Booking.BookingID);
                    if (booking != null)
                    {
                        booking.StatusID = 4; // Cancelled
                        await context.SaveChangesWithRetryAsync();
                        await LoadPendingCancellationsAsync(context);
                    }
                }
                return true;
            }, LoadingProgressBar, button);
            
            MessageBox.Show("Отмена подтверждена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ButtonDenyCancellation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null)
                return;

            await AsyncOperationHelper.RunWithProgressAsync(async () => {
                using (var context = new DBEntities())
                {
                    var booking = await context.Bookings.FindAsync(viewModel.Booking.BookingID);
                    if (booking != null)
                    {
                        booking.StatusID = 2; // Confirmed
                        await context.SaveChangesWithRetryAsync();
                        await LoadPendingCancellationsAsync(context);
                    }
                }
                return true;
            }, LoadingProgressBar, button);
            
            MessageBox.Show("Отмена отклонена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ButtonConfirmCompletion_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button.DataContext as ConfirmationViewModel;

            if (viewModel?.Booking == null || !viewModel.ReturnDateInput.HasValue || !viewModel.ActualCostInput.HasValue)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AsyncOperationHelper.RunWithProgressAsync(async () => {
                using (var context = new DBEntities())
                {
                    var booking = await context.Bookings.FindAsync(viewModel.Booking.BookingID);
                    if (booking != null)
                    {
                        booking.StatusID = 3; // Completed
                        booking.ReturnDate = viewModel.ReturnDateInput.Value;
                        booking.ActualCost = viewModel.ActualCostInput.Value;
                        await context.SaveChangesWithRetryAsync();
                        await LoadPendingCompletionsAsync(context);
                    }
                }
                return true;
            }, LoadingProgressBar, button);
            
            MessageBox.Show("Завершение подтверждено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }
}