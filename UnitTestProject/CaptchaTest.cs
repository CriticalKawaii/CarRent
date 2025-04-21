using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using WpfApp;


namespace UnitTestProject
{
    [TestClass]
    public class CaptchaTest
    {
        private SignInPage signInPage;

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
            signInPage = new SignInPage();
        }

        [TestMethod]
        [TestCategory("Captcha")]
        public void TestCaptchaAppearsAfterThreeFailedAttempts()
        {
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);

            Assert.IsFalse(string.IsNullOrEmpty(signInPage.GeneratedCaptcha));
        }

        [TestMethod]
        [TestCategory("Captcha")]
        public void TestSuccessfulLoginAfterCaptcha()
        {
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);

            bool result = signInPage.Authorize("user@email.com", "123456", signInPage.GeneratedCaptcha, skip: true);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestCategory("Captcha")]
        public void TestFailedLoginWithIncorrectCaptcha()
        {
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);
            signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);

            bool result = signInPage.Authorize("user@email.com", "123456", "WRONGCAPTCHA", skip: true);

            Assert.IsFalse(result);
        }
    }
}
