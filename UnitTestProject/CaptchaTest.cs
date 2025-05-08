using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using WpfApp;

namespace UnitTestProject
{
    //[TestClass]
    [TestCategory("Капча")]
    public class CaptchaTest
    {
        private SignInPage signInPage;
        [TestInitialize]
        
        public void Setup()
        {
            if (Application.Current == null)
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary{Source = new Uri("pack://application:,,,/WpfApp;component/ResourceDictionaries/RentACarStyle.xaml")});
            signInPage = new SignInPage();
        }
        private void TriggerCaptcha(){
            for (int i = 0; i < 3; i++) signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true); 
        }
        [TestMethod]
        public void TestCaptchaAppearsAfterThreeFailedAttempts_ReturnsFalse(){
            TriggerCaptcha();
            Assert.IsFalse(string.IsNullOrEmpty(signInPage.GeneratedCaptcha));
        }
        [TestMethod]
        public void TestSuccessfulLoginAfterCaptcha_ReturnsTrue()
        {
            TriggerCaptcha();
            Assert.IsTrue(signInPage.Authorize("user@email.com", "123456", signInPage.GeneratedCaptcha, skip: true));
        }
        [TestMethod]
        public void TestFailedLoginWithIncorrectCaptcha_ReturnsFalse()
        {
            TriggerCaptcha();
            Assert.IsFalse(signInPage.Authorize("user@email.com", "123456", "WRONGCAPTCHA", skip: true));
        }
    }
}