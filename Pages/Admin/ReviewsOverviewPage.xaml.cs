using System.Linq;
using System.Windows.Controls;

namespace WpfApp.Pages.admin
{

    public partial class ReviewsOverviewPage : Page
    {
        public ReviewsOverviewPage()
        {
            InitializeComponent();
            DataGridReviews.ItemsSource = DBEntities.GetContext().Reviews.ToList();
        }
    }
}
