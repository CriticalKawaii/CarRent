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
                frameAdmin.Navigate(new AdminDashboardPage());

                ChartBookings.ChartAreas.Add(new ChartArea("Main"));
                var currentSeries = new Series("Бронирования") { 
                    IsValueShownAsLabel = true
                };
                ChartBookings.Series.Add(currentSeries);

                ComboBoxVehicleCategory.ItemsSource = DBEntities.GetContext().VehicleCategories.ToList();
                ComboBoxDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));

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

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxDiagram.SelectedItem is SeriesChartType currentType) {
                Series currentSeries = ChartBookings.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                if (currentSeries == null)
                    return;
                
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var context = DBEntities.GetContext();

                var bookingCounts = context.Bookings
                    .GroupBy(b => b.Vehicle.VehicleCategory.VehicleCategory1)
                    .Select(g => new
                    {
                        VehicleCategory = g.Key,
                        BookingCount = g.Count(),
                    }).ToList();


                foreach (var item in bookingCounts)
                {
                    currentSeries.Points.AddXY(item.VehicleCategory, item.BookingCount);
                    

                    currentSeries.Points.Last().ToolTip = $"{item.VehicleCategory}: {item.BookingCount} бронирований";
                }
            }
        }

        private void ButtonWord_Click(object sender, RoutedEventArgs e)
        {
            var allVehicleCategories = DBEntities.GetContext().VehicleCategories.ToList();
            var allBookings = DBEntities.GetContext().Bookings.ToList();

            var application = new Word.Application();
            Word.Document document = application.Documents.Add();

            foreach (var vehicleCategory in allVehicleCategories)
            {
                Word.Paragraph vehicleCategoryParagraph = document.Paragraphs.Add();
                Word.Range vehicleCategoryRange = vehicleCategoryParagraph.Range;
                vehicleCategoryRange.Text = vehicleCategory.VehicleCategory1;
                vehicleCategoryParagraph.set_Style("Заголовок");
                vehicleCategoryRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                vehicleCategoryRange.InsertParagraphAfter();
                document.Paragraphs.Add(/*vehicleCategoryParagraph*/);
                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;
                Word.Table bookingsTable = document.Tables.Add(tableRange, allVehicleCategories.Count()+1,2);
                bookingsTable.Borders.InsideLineStyle = bookingsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                bookingsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                Word.Range cellRange;
                cellRange = bookingsTable.Cell(1, 1).Range;
                cellRange.Text = "Категория";
                cellRange = bookingsTable.Cell(1, 2).Range;
                cellRange.Text = "Бронирования";

                bookingsTable.Rows[1].Range.Font.Name = "Times New Roman";
                bookingsTable.Rows[1].Range.Font.Size = 14;
                bookingsTable.Rows[1].Range.Bold = 1;
                bookingsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                for(int i = 0; i < allVehicleCategories.Count(); i++)
                {

                }
            }
        }

        private void ButtonExcel_Click(object sender, RoutedEventArgs e)
        {

        }

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
    }
}
