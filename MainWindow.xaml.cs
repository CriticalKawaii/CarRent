using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp.Classes;
using WpfApp.Pages;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        ExploreCarsPage ExploreCarsPage { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ExploreCarsPage = new ExploreCarsPage();
            RadioButtonExplore.IsChecked = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private async void RadioButtonAccount_Checked(object sender, RoutedEventArgs e)
        {
                if (!SessionManager.IsLoggedIn)
                    frameMainWindow?.Navigate(new SignInPage());
                else
                {
                    frameMainWindow?.Navigate(new AccountPage());
                }
        }

        private async void RadioButtonExplore_Checked(object sender, RoutedEventArgs e) => frameMainWindow?.Navigate(ExploreCarsPage);

        private async void frameMainWindow_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) => LoadingProgressBar.Visibility = Visibility.Collapsed;
        
        private void frameMainWindow_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) => LoadingProgressBar.Visibility = Visibility.Visible;
    }
}
