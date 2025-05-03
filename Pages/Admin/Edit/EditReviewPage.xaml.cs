using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp.Pages.Admin.Edit
{
    public partial class EditReviewPage : Page
    {
        private Review _review = new Review();

        public EditReviewPage(Review selectedReview)
        {
            InitializeComponent();
            if (selectedReview != null)
            {
                _review = selectedReview;
            }
            DataContext = _review;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var context = DBEntities.GetContext();
            ComboBoxUsers.ItemsSource = context.Users.ToList();
            ComboBoxVehicles.ItemsSource = context.Vehicles.ToList();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (_review.User == null)
                errors.AppendLine("Выберите пользователя!");
            if (_review.Vehicle == null)
                errors.AppendLine("Выберите транспорт!");
            if (_review.Rating < 1 || _review.Rating > 5)
                errors.AppendLine("Введите корректный рейтинг (1-5)!");
            if (string.IsNullOrWhiteSpace(_review.Comment))
                errors.AppendLine("Введите комментарий!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_review.ReviewID == 0)
            {
                _review.CreatedAt = DateTime.Now;
                DBEntities.GetContext().Reviews.Add(_review);
            }

            try
            {
                DBEntities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}