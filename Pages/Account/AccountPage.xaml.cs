using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Classes;
using WpfApp.Pages.admin;
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using WpfApp.Pages.Account;


namespace WpfApp.Pages
{
    /// <summary>
    /// Interaction logic for AccountPage.xaml
    /// </summary>
    public partial class AccountPage : Page
    {
        public AccountPage()
        {
            InitializeComponent();
            if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.RoleID != 1)
            {
                TabItemAdministration.Visibility = Visibility.Visible;
                TabItemReports.Visibility = Visibility.Visible;

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
            }
        }

        private void ButtonReview_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReviewPage((sender as Button).DataContext as Booking));
        }

        private void ButtonUsersPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new UsersOverviewPage());
        }

        private void ButtonVehiclesPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new VehiclesOverviewPage());
        }

        private void ButtonBookingsPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new BookingsOverviewPage());
        }

        private void ButtonReviewsPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new ReviewsOverviewPage());
        }

        private void ButtonPaymentsPage_Checked(object sender, RoutedEventArgs e)
        {
            frameAdmin.Navigate(new PaymentsOverviewPage());
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
