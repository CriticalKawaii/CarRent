using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using WpfApp;

namespace UnitTestProject
{
    [TestClass]
    public class RegistrationTestCases
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
        [TestCategory("Registration Test Cases")]
        public void RegisterTest()
        {
            var signUpPage = new SignUpPage();

            // Тест-кейс 1: Успешная регистрация
            Assert.IsTrue(signUpPage.Register("ivanov@mail.com", "Иван", "Иванов", "SecurePass123", "SecurePass123", skip: true), "Ожидаемый результат: успешная регистрация");

            // Тест-кейс 2: Регистрация с пустым email
            Assert.IsFalse(signUpPage.Register("", "Иван", "Иванов", "SecurePass123", "SecurePass123", skip: true), "Ожидаемый результат: ошибка - email не может быть пустым");

            // Тест-кейс 3: Регистрация с некорректным email
            Assert.IsFalse(signUpPage.Register("ivanovmail.com", "Иван", "Иванов", "SecurePass123", "SecurePass123", skip: true), "Ожидаемый результат: ошибка - некорректный email");

            // Тест-кейс 4: Регистрация с несовпадающими паролями
            Assert.IsFalse(signUpPage.Register("ivanov@mail.com", "Иван", "Иванов", "SecurePass123", "SecurePass", skip: true), "Ожидаемый результат: ошибка - пароли не совпадают");

            // Тест-кейс 5: Регистрация с уже существующим email
            Assert.IsFalse(signUpPage.Register("user@email.com", "Иван", "Иванов", "SecurePass123", "SecurePass123", skip: true), "Ожидаемый результат: ошибка - email уже зарегистрирован");
        }
    }
}
