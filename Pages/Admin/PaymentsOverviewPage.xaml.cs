using System.Linq;
using System.Windows.Controls;

namespace WpfApp.Pages.admin
{

    public partial class PaymentsOverviewPage : Page
    {
        public PaymentsOverviewPage()
        {
            InitializeComponent();
            DataGridPayments.ItemsSource = DBEntities.GetContext().Payments.ToList();
        }
    }
}
