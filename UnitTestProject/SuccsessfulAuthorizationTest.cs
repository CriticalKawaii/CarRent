using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows;
using WpfApp;

namespace UnitTestProject
{
    //[TestClass]
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
            ("admin", "12345678"),         // Администратор (уже есть в БД)
            ("user@email.com", "123456"), // Обычный пользователь (уже есть в БД)
            
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
