using System;
using System.Diagnostics;
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                try
                {
                    e.Handled = true;

                    Process.Start("guide.chm");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии справки: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
