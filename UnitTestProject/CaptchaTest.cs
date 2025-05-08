using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfApp;

namespace UnitTestProject{
    [TestClass][TestCategory("Captcha")]
    public class CaptchaTest : WpfTestBase{
        private SignInPage signInPage;
        [TestInitialize]
        public void Setup(){
            signInPage = new SignInPage();
            TriggerCaptcha();
        }
        private void TriggerCaptcha(){
            for (int i = 0; i < 3; i++) signInPage.Authorize("wrong@example.com", "wrongpassword", skip: true);}
        [TestMethod("Captcha появилась после трех неуспешных попыток входа")]
        public void TestCaptchaAppearsAfterThreeFailedAttempts_ReturnsFalse() => Assert.IsFalse(string.IsNullOrEmpty(signInPage.GeneratedCaptcha));
        [TestMethod("Вход с неправильной Captcha")]
        public void TestFailedLoginWithIncorrectCaptcha_ReturnsFalse() => Assert.IsFalse(signInPage.Authorize("user@email.com", "123456", "WRONGCAPTCHA", skip: true));
        [TestMethod("Успешный вход c Captcha")]
        public void TestSuccessfulLoginAfterCaptcha_ReturnsTrue() =>Assert.IsTrue(signInPage.Authorize("user@email.com", "123456", signInPage.GeneratedCaptcha, skip: true));
    }
}