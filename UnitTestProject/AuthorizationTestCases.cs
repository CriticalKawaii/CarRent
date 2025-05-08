using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfApp;

namespace UnitTestProject{

    [TestClass][TestCategory("Авторизация")]
    public class AuthorizationTestCases : WpfTestBase{
        private SignInPage signInPage;
        [TestInitialize]
        public void Setup() => signInPage = new SignInPage();

        [DataTestMethod]
        [DataRow("user@email.com", "123456", true, DisplayName = "Корректные данные пользователя ")]
        [DataRow("admin", "12345678", true, DisplayName = "Корректные данные администратора ")]
        [DataRow("qwerty", "asdfg", false, DisplayName = "Несуществующий пользователь")]
        [DataRow("user@email.com", "wrongpass", false, DisplayName = "Неверный пароль")]
        [DataRow("", "", false, DisplayName = "Пустые поля")] 
        public void Authorize_WithVariousCredentials_ReturnsExpectedResults(string email, string password, bool expectedResult) =>
            Assert.AreEqual(expectedResult, signInPage.Authorize(email, password, skip: true), $"Авторизация с {email}/{password} должна вернуть {expectedResult}");
    }
}
