using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp.Pages.Admin.Edit
{

    public partial class EditUserPage : Page
    {
        private User _user = new User();
        private readonly string _originalPassword;

        public EditUserPage(User selectedUser)
        {
            InitializeComponent();
            if (selectedUser != null)
            {
                _user = selectedUser;
                _originalPassword = selectedUser.PasswordHash;
            }
            DataContext = _user;
            Loaded += Page_Loaded;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxPassword.Text = null;
            ComboBoxRoles.ItemsSource = DBEntities.GetContext().Roles.ToList();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(s => s.ToString("X2")));
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (_user.Role == null)
                errors.AppendLine("Выберите роль!");
            if (string.IsNullOrWhiteSpace(_user.Email))
                errors.AppendLine("Введите E-mail!");
            if (string.IsNullOrWhiteSpace(_user.FirstName))
                errors.AppendLine("Введите имя!");
            if (string.IsNullOrWhiteSpace(_user.LastName))
                errors.AppendLine("Введите фамилию!");
            if (_user.UserID == 0 && string.IsNullOrWhiteSpace(_user.PasswordHash))
                errors.AppendLine("Введите пароль!");


            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_user.UserID == 0)
            {
                _user.CreatedAt = DateTime.Now;
                _user.PasswordHash = GetHash(_user.PasswordHash);
                DBEntities.GetContext().Users.Add(_user);
            }
            else
            {
                if (string.IsNullOrEmpty(_user.PasswordHash))
                {
                    _user.PasswordHash = _originalPassword;
                }
                else
                {
                    _user.PasswordHash = GetHash(_user.PasswordHash);
                }
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
