using MaterialDesignThemes.Wpf;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WpfApp.Classes;
using WpfApp.Pages;

namespace WpfApp
{
    /// <summary>
    /// Represents the sign-in page for user authentication.
    /// </summary>
    /// <remarks>
    /// This page handles user authentication with email and password,
    /// implements CAPTCHA verification after failed attempts,
    /// and provides password reveal functionality.
    /// </remarks>
    public partial class SignInPage : Page
    {
        public MainWindow MainWindow {get; set;}
        private int failedAttempts = 0;
        public string GeneratedCaptcha { get; private set; } 

        public SignInPage()
        {
            InitializeComponent();
            Loaded += SignInPage_Loaded;
        }

        public void SignInPage_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxEmail.Focus();
            MainWindow = Application.Current.MainWindow as MainWindow;
        }

        /// <summary>
        /// Generates a SHA1 hash from the provided password.
        /// </summary>
        /// <param name="String">The password to hash.</param>
        /// <returns>A hexadecimal string representation of the SHA1 hash.</returns>
        private static string GetHash(string String)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(String)).Select(s => s.ToString("X2")));
            }
        }

        /// <summary>
        /// Generates a new CAPTCHA image with random alphanumeric text.
        /// </summary>
        /// <remarks>
        /// Creates a 5-character CAPTCHA string from uppercase letters and numbers,
        /// renders it as an image with interference lines, and displays it in the UI.
        /// </remarks>
        private void GenerateCaptcha()
        {
            var rand = new Random();
            GeneratedCaptcha = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 5)
                .Select(s => s[rand.Next(s.Length)]).ToArray());

            Dispatcher.Invoke(() =>
            {
                using (var bitmap = new Bitmap(150, 50))
                using (var g = Graphics.FromImage(bitmap))
                using (var ms = new MemoryStream())
                {
                    g.Clear(Color.White);
                    g.DrawString(GeneratedCaptcha, new Font("Arial", 20), Brushes.Black, new PointF(20, 10));
                    g.DrawLine(Pens.Orange, 0, 0, 150, 50);
                    g.DrawLine(Pens.Purple, 0, 50, 150, 0);
                    bitmap.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    captchaImage.Source = bitmapImage;
                }
            });
        }
        private void ShowCaptcha()
        {
            Dispatcher.Invoke(() =>
            {
                captchaImage.Visibility = Visibility.Visible;
                captchaInput.Visibility = Visibility.Visible;
                refreshCaptchaButton.Visibility = Visibility.Visible;
                captchaErrorText.Text = "";
                GenerateCaptcha();
            });
        }

        /// <summary>
        /// Authenticates a user with the provided credentials.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password (plain text).</param>
        /// <param name="captchaInputText">The CAPTCHA input text (required after 3 failed attempts).</param>
        /// <param name="skip">If true, skips UI notifications and navigation.</param>
        /// <returns>True if authentication is successful; otherwise, false.</returns>
        /// <exception cref="Exception">Thrown when database connection fails.</exception>
        public bool Authorize(string email, string password, string captchaInputText = "", bool skip = false)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                if (!skip)
                    MessageBox.Show("Введите E-mail и пароль!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (failedAttempts >= 3 && (string.IsNullOrEmpty(captchaInputText) || captchaInputText.ToUpper() != GeneratedCaptcha))
            {
                captchaErrorText.Text = "Неверная капча!";
                return false;
            }

            string hashPassword = GetHash(password);

            using (var db = new DBEntities())
            {
                User user;
                try
                {
                    user = db.Users.AsNoTracking().FirstOrDefault(u => u.Email.Trim() == email && u.PasswordHash.Trim() == hashPassword);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Проверьте подключение к интернету: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (user == null)
                {
                    failedAttempts++;
                    if (failedAttempts >= 3) ShowCaptcha();
                    if (!skip) MessageBox.Show("Пользователь не найден!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                failedAttempts = 0;
                SessionManager.SignIn(user);

                if (!skip)
                    NavigationService.Navigate(new AccountPage());
                return true;
            }
        }

        private void ButtonSignIn_Click(object sender, RoutedEventArgs e)
        {
            checkBoxRevealPassword.IsChecked = false;
            string email = textBoxEmail.Text.Trim();
            string password = passwordBoxPassword.Password.Trim();
            string captchaText = captchaImage.Visibility == Visibility.Visible ? captchaInput.Text.Trim() : "";
            Authorize(email, password, captchaText);
            if (MainWindow != null)
                MainWindow.RadioButtonAccountIcon.Kind = PackIconKind.Account;
        }

        private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            captchaErrorText.Text = "";
            GenerateCaptcha();
        }

        private void ButtonSignUp_Click(object sender, RoutedEventArgs e)
        {            
            NavigationService.Navigate(new SignUpPage());
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            textBoxRevealedPassword.Text = passwordBoxPassword.Password;
            passwordBoxPassword.Visibility = Visibility.Collapsed;
            textBoxRevealedPassword.Visibility = Visibility.Visible;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            passwordBoxPassword.Password = textBoxRevealedPassword.Text;
            textBoxRevealedPassword.Visibility = Visibility.Collapsed;
            passwordBoxPassword.Visibility = Visibility.Visible;
        }
    }
}
