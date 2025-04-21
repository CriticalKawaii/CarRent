using System.Linq;
using System.Windows.Controls;

namespace WpfApp.Pages.admin
{
    public partial class BookingsOverviewPage : Page
    {
        public BookingsOverviewPage()
        {
            InitializeComponent();
            DataGridBookings.ItemsSource = DBEntities.GetContext().Bookings.ToList();
        }
    }
}
