using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows;
using WpfApp;

namespace UnitTestProject
{
    public class SuccsessfulAuthorizationTest
    {
        [TestInitialize]
        public void Setup()
        {
            if (Application.Current == null)
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {Source = new Uri("pack://application:,,,/WpfApp;component/ResourceDictionaries/RentACarStyle.xaml")});
        }
        private List<(string Email, string Password)> testUsers = new List<(string, string)>
        {
            ("Admin", "12345678"),         // Администратор (уже есть в БД)
            ("user@email.com", "123456"), // Обычный пользователь (уже есть в БД)
            ("test1@email.com", "Password1"),
            ("test2@email.com", "Password2"),
            ("test3@email.com", "Password3"),
        };
        [TestMethod][TestCategory("Succsessful Authorization Test Across All Users")]
        public void AuthTestSuccess()
        {
            var signInPage = new SignInPage();
            foreach (var (Email, Password) in testUsers)
                Assert.IsTrue(signInPage.Authorize(Email, Password, skip: true), $"Ошибка авторизации {Email}");
        }
    }
}
