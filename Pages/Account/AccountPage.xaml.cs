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


namespace WpfApp.Pages
{
    public partial class AccountPage : Page
    {
        public AccountPage()
        {
            InitializeComponent();
            if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.RoleID != 1)
            {
                TabItemAdministration.Visibility = Visibility.Visible;
                TabItemReports.Visibility = Visibility.Visible;
                TabItemConfirmations.Visibility = Visibility.Visible;
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

        
    }
}
