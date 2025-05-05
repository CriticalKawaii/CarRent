using System.Collections.Generic;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp
{
    public partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            InitializeComponent();
            textBoxEmail.Focus();
        }
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(s => s.ToString("X2")));
            }
        }
        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$");
        }

        public bool Register(string email, string firstName, string lastName, string password, string passwordRepeat, bool skip = false)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordRepeat))
            {
                if (!skip)
                    MessageBox.Show("Заполните все поля", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(email))
            {
                if (!skip)
                    MessageBox.Show("E-mail не может быть пустым", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!IsValidEmail(email))
            {
                if (!skip)
                    MessageBox.Show("Некорректный E-mail", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using (var db = new DBEntities())
            {
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    if (!skip)
                        MessageBox.Show("Аккаунт с этой электронной почтой уже существует", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (string.IsNullOrEmpty(firstName))
                    errors.Add("Имя не может быть пустым.");
                else if (!Regex.IsMatch(firstName, @"^[a-zA-Zа-яА-Я\- ]+$"))
                    errors.Add("Имя может содержать только буквы, дефисы и пробелы.");
                if (string.IsNullOrEmpty(lastName))
                    errors.Add("Фамилия не может быть пустой.");
                else if (!Regex.IsMatch(lastName, @"^[a-zA-Zа-яА-Я\- ]+$"))
                    errors.Add("Фамилия может содержать только буквы, дефисы и пробелы.");

                if (password.Length < 6)
                { errors.Add("Пароль должен быть больше 6 символов"); }
                if (!Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"))
                { errors.Add("Пароль должен быть на английском языке и содержать только буквы и цифры"); }
                if (!password.Any(char.IsDigit))
                { errors.Add("Пароль должен содержать хотя бы одну цифру"); }
                if (password != passwordRepeat.Trim())
                { errors.Add("Пароли не совпадают"); }

                if (errors.Any())
                {
                    if (!skip)
                        MessageBox.Show(string.Join(Environment.NewLine, errors), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false; 
                }
                if (!skip) { 
                    var userObject = new User
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        PasswordHash = GetHash(password),
                        RoleID = 1,
                        Email = email,
                        CreatedAt = DateTime.Now,
                    };

                    db.Users.Add(userObject);
                    db.SaveChanges();
                    
                    MessageBox.Show("Вы успешно зарегистрировались!", "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.Navigate(new SignInPage());
                }
                return true;
            }
        }
        private void ButtonSignUp_Click(object sender, RoutedEventArgs e)
        {
            checkBoxRevealPassword.IsChecked = false;
            var email = textBoxEmail.Text.Trim();
            var firstName = textBoxFirstName.Text.Trim();
            var lastName = textBoxLastName.Text.Trim();
            var password = passwordBoxPassword.Password.Trim();
            var passwordRepeat = passwordBoxPasswordRepeat.Password.Trim();
            Register(email, firstName, lastName, password, passwordRepeat);
        }
        private void ButtonSignIn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SignInPage());
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
