using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfApp.Pages;
using System;
using WpfApp;
using System.Windows;
using System.Windows.Navigation;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject
{
    [TestClass]
    public class AuthorizationTestCases
    {
        [TestInitialize]
        public void Setup()
        {
            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }

            Application.Current.Resources.MergedDictionaries.Clear(); 
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/WpfApp;component/ResourceDictionaries/RentACarStyle.xaml")
            });
        }

        [TestMethod]
        [TestCategory("Authorization Test Cases")]
        public void AuthorizeTest()
        {
            var signInPage = new SignInPage();

            Assert.IsTrue(signInPage.Authorize("user@email.com", "123456", skip: true));
            Assert.IsFalse(signInPage.Authorize("test", "test", skip: true));
            Assert.IsFalse(signInPage.Authorize("","", skip: true));
            Assert.IsFalse(signInPage.Authorize(" "," ", skip: true));

            // Тест-кейс 1: Авторизация с валидными данными пользователя
            Assert.IsTrue(signInPage.Authorize("user@email.com", "123456", skip: true), "Ожидаемый результат: успешная авторизация");

            // Тест-кейс 2: Авторизация администратора
            Assert.IsTrue(signInPage.Authorize("Admin", "12345678", skip: true), "Ожидаемый результат: успешная авторизация администратора");

            // Тест-кейс 3: Авторизация с неправильным email
            Assert.IsFalse(signInPage.Authorize("qwerty", "123456", skip: true), "Ожидаемый результат: пользователь не найден");

            // Тест-кейс 4: Авторизация с неправильным паролем
            Assert.IsFalse(signInPage.Authorize("user@email.com", "69100500", skip: true), "Ожидаемый результат: неверный пароль");

            // Тест-кейс 5: Авторизация с пустыми полями
            Assert.IsFalse(signInPage.Authorize("", "", skip: true), "Ожидаемый результат: ошибка ввода");

            // Тест-кейс 6: Авторизация с пробелами вместо данных
            Assert.IsFalse(signInPage.Authorize(" ", " ", skip: true), "Ожидаемый результат: ошибка ввода");
        }
    }
}
