using System.Windows;
using System.Windows.Input;
using WpfApp.Classes;
using WpfApp.Pages;
using WpfApp.Pages.admin;

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

        private void RadioButtonAccount_Checked(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync( () => {
                if (!SessionManager.IsLoggedIn)
                    frameMainWindow?.Navigate(new SignInPage());
                else
                {
                    frameMainWindow?.Navigate(new AccountPage());
                }
            } );
        }

        private void RadioButtonExplore_Checked(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync( () => { frameMainWindow?.Navigate(ExploreCarsPage); } );
        }
    }
}
