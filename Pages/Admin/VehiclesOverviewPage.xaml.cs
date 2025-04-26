using System.Linq;
using System.Windows.Controls;

namespace WpfApp.Pages.admin
{

    public partial class VehiclesOverviewPage : Page
    {
        public VehiclesOverviewPage()
        {
            InitializeComponent();
            DataGridVehicles.ItemsSource = DBEntities.GetContext().Vehicles.ToList();
        }


    }
}
