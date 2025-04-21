using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp.Pages.Admin.Edit
{
    /// <summary>
    /// Interaction logic for EditUsersPage.xaml
    /// </summary>
    public partial class EditUsersPage : Page
    {
        public EditUsersPage(User selectedUser)
        {
            InitializeComponent();
            if (selectedUser != null) 
                _user = selectedUser;
            DataContext = _user;
            ComboBoxRoles.ItemsSource = DBEntities.GetContext().Roles.ToList();
        }
        private User _user = new User();

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
            if (string.IsNullOrWhiteSpace(_user.PasswordHash))
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

            try
            {
                DBEntities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
